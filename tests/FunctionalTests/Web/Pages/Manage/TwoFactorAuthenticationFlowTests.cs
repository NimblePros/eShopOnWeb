using System.Buffers.Binary;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Manage;

/// <summary>
/// Walks the authenticator / recovery code / disable pages end to end with a throwaway user, so
/// enabling 2FA doesn't affect the demo user that other tests sign in as.
/// </summary>
[Collection("Sequential")]
public class TwoFactorAuthenticationFlowTests(TestApplication factory) : IClassFixture<TestApplication>
{
    [Fact]
    public async Task EnablesAuthenticatorThenRegeneratesRecoveryCodesThenDisables2fa()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await LoginAsync(client, email);

        // Enable the authenticator with a valid code.
        var enablePage = await client.GetStringAsync("/manage/enable-authenticator", ct);
        var enableResponse = await PostFormAsync(client, "/manage/enable-authenticator", enablePage,
            KeyValuePair.Create("Code", await GetCurrentTotpAsync(email)));
        Assert.Equal(HttpStatusCode.Redirect, enableResponse.StatusCode);
        Assert.Equal("/manage/show-recovery-codes", enableResponse.Headers.Location!.OriginalString);

        var codesPage = await client.GetStringAsync("/manage/show-recovery-codes", ct);
        Assert.Equal(10, CountRecoveryCodes(codesPage));

        // The codes are only shown once.
        var revisit = await client.GetAsync("/manage/show-recovery-codes", ct);
        Assert.Equal(HttpStatusCode.Redirect, revisit.StatusCode);

        var overview = await client.GetStringAsync("/manage/two-factor-authentication", ct);
        Assert.Contains("href=\"/manage/disable-2fa\"", overview);
        Assert.Contains("href=\"/manage/generate-recovery-codes\"", overview);

        // Regenerate recovery codes.
        var generatePage = await client.GetStringAsync("/manage/generate-recovery-codes", ct);
        var generateResponse = await PostFormAsync(client, "/manage/generate-recovery-codes", generatePage);
        Assert.Equal(HttpStatusCode.Redirect, generateResponse.StatusCode);
        Assert.Equal("/manage/show-recovery-codes", generateResponse.Headers.Location!.OriginalString);
        Assert.Equal(10, CountRecoveryCodes(await client.GetStringAsync("/manage/show-recovery-codes", ct)));

        // Disable 2FA.
        var disablePage = await client.GetStringAsync("/manage/disable-2fa", ct);
        var disableResponse = await PostFormAsync(client, "/manage/disable-2fa", disablePage);
        Assert.Equal(HttpStatusCode.Redirect, disableResponse.StatusCode);
        Assert.Equal("/manage/two-factor-authentication", disableResponse.Headers.Location!.OriginalString);
        Assert.False((await FindUserAsync(factory, email)).TwoFactorEnabled);
    }

    [Fact]
    public async Task EnableAuthenticatorRejectsInvalidCode()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await LoginAsync(client, email);

        var enablePage = await client.GetStringAsync("/manage/enable-authenticator", ct);
        var response = await PostFormAsync(client, "/manage/enable-authenticator", enablePage, KeyValuePair.Create("Code", "000000x"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Verification code is invalid.", await response.Content.ReadAsStringAsync(ct));
        Assert.False((await FindUserAsync(factory, email)).TwoFactorEnabled);
    }

    private static int CountRecoveryCodes(string page) =>
        Regex.Matches(page, @"<code class=""recovery-code"">").Count;

    /// <summary>
    /// Computes the current RFC 6238 code (HMAC-SHA1, 30 second step, 6 digits) the way an
    /// authenticator app would from the user's shared key.
    /// </summary>
    private async Task<string> GetCurrentTotpAsync(string email)
    {
        var key = await UseUserManagerAsync(factory, async userManager =>
            await userManager.GetAuthenticatorKeyAsync((await userManager.FindByEmailAsync(email))!));

        Span<byte> timestep = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(timestep, DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30);
        var hash = HMACSHA1.HashData(FromBase32(key!), timestep);

        var offset = hash[^1] & 0x0F;
        var binary = (BinaryPrimitives.ReadInt32BigEndian(hash.AsSpan(offset, 4)) & 0x7FFFFFFF) % 1_000_000;
        return binary.ToString("D6");
    }

    private static byte[] FromBase32(string input)
    {
        const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
        var bytes = new List<byte>();
        int buffer = 0, bitsLeft = 0;
        foreach (var c in input.TrimEnd('=').ToUpperInvariant())
        {
            buffer = (buffer << 5) | alphabet.IndexOf(c);
            bitsLeft += 5;
            if (bitsLeft >= 8)
            {
                bytes.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }
        return [.. bytes];
    }
}
