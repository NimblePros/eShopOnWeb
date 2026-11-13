using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Manage;

/// <summary>
/// Profile, password and external login flows, each run as a throwaway user.
/// </summary>
[Collection("Sequential")]
public class AccountManagementFlowTests(TestApplication factory) : IClassFixture<TestApplication>
{
    private async Task<(HttpClient Client, string Email)> SignInNewUserAsync()
    {
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await LoginAsync(client, email);
        return (client, email);
    }

    [Fact]
    public async Task SendVerificationEmailShowsConfirmation()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        var profilePage = await client.GetStringAsync("/manage/my-account", ct);
        Assert.Contains("Send verification email", profilePage);

        var response = await PostFormAsync(client, "/manage/my-account?handler=SendVerificationEmail", profilePage,
            KeyValuePair.Create("Email", email));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/manage/my-account", response.Headers.Location!.OriginalString);
        Assert.Contains("Verification email sent. Please check your email.",
            await client.GetStringAsync("/manage/my-account", ct));
    }

    [Fact]
    public async Task SetPasswordAddsPasswordForUserWithoutOne()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        await UseUserManagerAsync(factory, async userManager =>
            await userManager.RemovePasswordAsync((await userManager.FindByEmailAsync(email))!));

        var changePasswordRedirect = await client.GetAsync("/manage/change-password", ct);
        Assert.Equal("/manage/set-password", changePasswordRedirect.Headers.Location!.OriginalString);

        var setPasswordPage = await client.GetStringAsync("/manage/set-password", ct);
        var response = await PostFormAsync(client, "/manage/set-password", setPasswordPage,
            KeyValuePair.Create("NewPassword", "NewPass@word1"),
            KeyValuePair.Create("ConfirmPassword", "NewPass@word1"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/manage/change-password", response.Headers.Location!.OriginalString);
        Assert.Contains("Your password has been set.", await client.GetStringAsync("/manage/change-password", ct));
        Assert.True(await UseUserManagerAsync(factory, async userManager =>
            await userManager.HasPasswordAsync((await userManager.FindByEmailAsync(email))!)));
    }

    [Fact]
    public async Task RemoveLoginRemovesExternalLoginGivenUserWithPassword()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        await AddExternalLoginAsync(email);
        var page = await client.GetStringAsync("/manage/external-logins", ct);

        var response = await PostFormAsync(client, "/manage/external-logins?handler=RemoveLogin", page,
            KeyValuePair.Create("LoginProvider", "GitHub"),
            KeyValuePair.Create("ProviderKey", "github-key"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("The external login was removed.", await client.GetStringAsync("/manage/external-logins", ct));
        Assert.Empty(await GetLoginsAsync(email));
    }

    [Fact]
    public async Task RemoveLoginRefusesToRemoveOnlySignInMethod()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        await AddExternalLoginAsync(email);
        await UseUserManagerAsync(factory, async userManager =>
            await userManager.RemovePasswordAsync((await userManager.FindByEmailAsync(email))!));
        var page = await client.GetStringAsync("/manage/external-logins", ct);
        Assert.DoesNotContain(">Remove</button>", page);

        var response = await PostFormAsync(client, "/manage/external-logins?handler=RemoveLogin", page,
            KeyValuePair.Create("LoginProvider", "GitHub"),
            KeyValuePair.Create("ProviderKey", "github-key"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("You can&#x27;t remove your only way to sign in.", await client.GetStringAsync("/manage/external-logins", ct));
        Assert.Single(await GetLoginsAsync(email));
    }

    private Task<IdentityResult> AddExternalLoginAsync(string email) =>
        UseUserManagerAsync(factory, async userManager =>
            await userManager.AddLoginAsync((await userManager.FindByEmailAsync(email))!,
                new UserLoginInfo("GitHub", "github-key", "GitHub")));

    private Task<IList<UserLoginInfo>> GetLoginsAsync(string email) =>
        UseUserManagerAsync(factory, async userManager =>
            await userManager.GetLoginsAsync((await userManager.FindByEmailAsync(email))!));
}
