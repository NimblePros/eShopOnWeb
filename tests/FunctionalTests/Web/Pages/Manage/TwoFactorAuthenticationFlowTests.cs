using System.Buffers.Binary;
using System.Net;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Manage;

/// <summary>
/// Walks the authenticator / recovery code / disable pages end to end with a throwaway user, so
/// enabling 2FA doesn't affect the demo user that other tests sign in as.
/// </summary>
[Collection("Sequential")]
public class TwoFactorAuthenticationFlowTests(TestApplication factory) : IClassFixture<TestApplication>
{
    private const string Password = "Pass@word1";

    [Fact]
    public async Task EnablesAuthenticatorThenRegeneratesRecoveryCodesThenDisables2fa()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"2fa-{Guid.NewGuid():N}@example.com";
        await CreateUserAsync(email);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await WebPageHelpers.LoginAsync(client, email, Password);

        // Enable the authenticator with a valid code.
        var enablePage = await client.GetStringAsync("/manage/enable-authenticator", ct);
        var enableResponse = await PostAsync(client, "/manage/enable-authenticator", enablePage,
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
        var generateResponse = await PostAsync(client, "/manage/generate-recovery-codes", generatePage);
        Assert.Equal(HttpStatusCode.Redirect, generateResponse.StatusCode);
        Assert.Equal("/manage/show-recovery-codes", generateResponse.Headers.Location!.OriginalString);
        Assert.Equal(10, CountRecoveryCodes(await client.GetStringAsync("/manage/show-recovery-codes", ct)));

        // Disable 2FA.
        var disablePage = await client.GetStringAsync("/manage/disable-2fa", ct);
        var disableResponse = await PostAsync(client, "/manage/disable-2fa", disablePage);
        Assert.Equal(HttpStatusCode.Redirect, disableResponse.StatusCode);
        Assert.Equal("/manage/two-factor-authentication", disableResponse.Headers.Location!.OriginalString);
        Assert.False((await FindUserAsync(email)).TwoFactorEnabled);
    }

    [Fact]
    public async Task EnableAuthenticatorRejectsInvalidCode()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = $"2fa-{Guid.NewGuid():N}@example.com";
        await CreateUserAsync(email);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await WebPageHelpers.LoginAsync(client, email, Password);

        var enablePage = await client.GetStringAsync("/manage/enable-authenticator", ct);
        var response = await PostAsync(client, "/manage/enable-authenticator", enablePage, KeyValuePair.Create("Code", "000000x"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Verification code is invalid.", await response.Content.ReadAsStringAsync(ct));
        Assert.False((await FindUserAsync(email)).TwoFactorEnabled);
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, string url, string pageWithForm,
        params KeyValuePair<string, string>[] fields)
    {
        var values = new List<KeyValuePair<string, string>>(fields)
        {
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(pageWithForm))
        };
        return client.PostAsync(url, new FormUrlEncodedContent(values), TestContext.Current.CancellationToken);
    }

    private static int CountRecoveryCodes(string page) =>
        Regex.Matches(page, @"<code class=""recovery-code"">").Count;

    private async Task CreateUserAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var result = await userManager.CreateAsync(new ApplicationUser { UserName = email, Email = email }, Password);
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    private async Task<ApplicationUser> FindUserAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        return (await userManager.FindByEmailAsync(email))!;
    }

    /// <summary>
    /// Computes the current RFC 6238 code (HMAC-SHA1, 30 second step, 6 digits) the way an
    /// authenticator app would from the user's shared key.
    /// </summary>
    private async Task<string> GetCurrentTotpAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByEmailAsync(email);
        var key = await userManager.GetAuthenticatorKeyAsync(user!);

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
