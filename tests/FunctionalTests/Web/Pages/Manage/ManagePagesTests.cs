using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Manage;

[Collection("Sequential")]
public class ManagePagesTests(TestApplication factory) : IClassFixture<TestApplication>
{
    private readonly HttpClient _client = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false
    });

    [Theory]
    [InlineData("/manage/my-account")]
    [InlineData("/manage/change-password")]
    [InlineData("/manage/set-password")]
    [InlineData("/manage/external-logins")]
    [InlineData("/manage/two-factor-authentication")]
    [InlineData("/manage/enable-authenticator")]
    [InlineData("/manage/reset-authenticator")]
    [InlineData("/manage/show-recovery-codes")]
    public async Task RedirectsAnonymousUserToLogin(string url)
    {
        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("/manage/my-account", "Profile")]
    [InlineData("/manage/change-password", "Change password")]
    [InlineData("/manage/external-logins", "Manage your account")]
    [InlineData("/manage/two-factor-authentication", "Two-factor authentication")]
    [InlineData("/manage/enable-authenticator", "Enable authenticator")]
    [InlineData("/manage/reset-authenticator", "Reset authenticator key")]
    public async Task ReturnsPageGivenSignedInUser(string url, string expectedText)
    {
        await WebPageHelpers.LoginAsync(_client);

        var response = await _client.GetAsync(url, TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(expectedText, content);
    }

    [Fact]
    public async Task MyAccountRendersNavigationLinksToOtherManagePages()
    {
        await WebPageHelpers.LoginAsync(_client);

        var content = await _client.GetStringAsync("/manage/my-account", TestContext.Current.CancellationToken);

        Assert.Contains("href=\"/manage/my-account\"", content);
        Assert.Contains("href=\"/manage/change-password\"", content);
        Assert.Contains("href=\"/manage/two-factor-authentication\"", content);
    }

    [Fact]
    public async Task SetPasswordRedirectsToChangePasswordGivenUserWithPassword()
    {
        await WebPageHelpers.LoginAsync(_client);

        var response = await _client.GetAsync("/manage/set-password", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/manage/change-password", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task ShowRecoveryCodesRedirectsWhenNoCodesWereJustGenerated()
    {
        await WebPageHelpers.LoginAsync(_client);

        var response = await _client.GetAsync("/manage/show-recovery-codes", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/manage/two-factor-authentication", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task UpdatesPhoneNumber()
    {
        var ct = TestContext.Current.CancellationToken;
        await WebPageHelpers.LoginAsync(_client);
        var profilePage = await _client.GetStringAsync("/manage/my-account", ct);

        var postResponse = await _client.PostAsync("/manage/my-account", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("Email", WebPageHelpers.DemoUserEmail),
            new("PhoneNumber", "03656565"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(profilePage))
        }), ct);

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        var updatedProfilePage = await _client.GetStringAsync("/manage/my-account", ct);
        Assert.Contains("03656565", updatedProfilePage);
        Assert.Contains("Your profile has been updated", updatedProfilePage);
    }

    [Fact]
    public async Task ChangePasswordShowsErrorGivenWrongCurrentPassword()
    {
        var ct = TestContext.Current.CancellationToken;
        await WebPageHelpers.LoginAsync(_client);
        var page = await _client.GetStringAsync("/manage/change-password", ct);

        var response = await _client.PostAsync("/manage/change-password", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("OldPassword", "not-the-password"),
            new("NewPassword", "NewPass@word1"),
            new("ConfirmPassword", "NewPass@word1"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(page))
        }), ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Incorrect password.", await response.Content.ReadAsStringAsync(ct));
    }

    [Fact]
    public async Task RejectsPostWithoutAntiforgeryToken()
    {
        await WebPageHelpers.LoginAsync(_client);

        var response = await _client.PostAsync("/manage/my-account", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("Email", WebPageHelpers.DemoUserEmail),
            new("PhoneNumber", "1234567")
        }), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
