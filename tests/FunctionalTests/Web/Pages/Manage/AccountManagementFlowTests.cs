using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Web.Pages.Manage;
using Microsoft.Extensions.Logging;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Manage;

/// <summary>
/// Profile, password and external login flows, each run as a throwaway user.
/// </summary>
[Collection(WebTestCollection.Name)]
public class AccountManagementFlowTests(TestApplication factory)
{
    private async Task<(HttpClient Client, string Email)> SignInNewUserAsync()
    {
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(allowAutoRedirect: false);
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
        await AssertLoggedAsync<SetPasswordModel>(email, LogLevel.Information, "set a password");
    }

    [Fact]
    public async Task ChangingEmailLogsTheChangeWithoutTheAddress()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        var userId = (await FindUserAsync(factory, email)).Id;
        var newEmail = $"changed-{Guid.NewGuid():N}@example.com";
        var profilePage = await client.GetStringAsync("/manage/my-account", ct);

        var response = await PostFormAsync(client, "/manage/my-account", profilePage,
            KeyValuePair.Create("Email", newEmail));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var entry = Assert.Single(EntriesFor<MyAccountModel>(userId), e => e.Message.Contains("changed their email"));
        Assert.Equal(LogLevel.Information, entry.Level);
        Assert.DoesNotContain(factory.Logs.Entries, e => e.Message.Contains(newEmail));
    }

    [Fact]
    public async Task RemoveLoginRemovesExternalLoginGivenUserWithPassword()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        var providerKey = await AddExternalLoginAsync(email);
        var page = await client.GetStringAsync("/manage/external-logins", ct);

        var response = await PostFormAsync(client, "/manage/external-logins?handler=RemoveLogin", page,
            KeyValuePair.Create("LoginProvider", "GitHub"),
            KeyValuePair.Create("ProviderKey", providerKey));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("The external login was removed.", await client.GetStringAsync("/manage/external-logins", ct));
        Assert.Empty(await GetLoginsAsync(email));
        await AssertLoggedAsync<ExternalLoginsModel>(email, LogLevel.Information, "removed an external login from GitHub");
    }

    [Fact]
    public async Task RemoveLoginRefusesToRemoveOnlySignInMethod()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        var providerKey = await AddExternalLoginAsync(email);
        await UseUserManagerAsync(factory, async userManager =>
            await userManager.RemovePasswordAsync((await userManager.FindByEmailAsync(email))!));
        var page = await client.GetStringAsync("/manage/external-logins", ct);
        Assert.DoesNotContain(">Remove</button>", page);

        var response = await PostFormAsync(client, "/manage/external-logins?handler=RemoveLogin", page,
            KeyValuePair.Create("LoginProvider", "GitHub"),
            KeyValuePair.Create("ProviderKey", providerKey));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("You can&#x27;t remove your only way to sign in.", await client.GetStringAsync("/manage/external-logins", ct));
        Assert.Single(await GetLoginsAsync(email));
        await AssertLoggedAsync<ExternalLoginsModel>(email, LogLevel.Warning, "Refused to remove the only sign-in method (GitHub)");
    }

    [Fact]
    public async Task RemoveLoginReportsErrorGivenLoginTheUserDoesNotHave()
    {
        var ct = TestContext.Current.CancellationToken;
        var (client, email) = await SignInNewUserAsync();
        await AddExternalLoginAsync(email);
        var page = await client.GetStringAsync("/manage/external-logins", ct);

        var response = await PostFormAsync(client, "/manage/external-logins?handler=RemoveLogin", page,
            KeyValuePair.Create("LoginProvider", "GitHub"),
            KeyValuePair.Create("ProviderKey", "someone-elses-key"));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("The external login to remove was not found.", await client.GetStringAsync("/manage/external-logins", ct));
        Assert.Single(await GetLoginsAsync(email));
        await AssertLoggedAsync<ExternalLoginsModel>(email, LogLevel.Warning, "tried to remove an external login they don't have");
    }

    private IEnumerable<CapturedLogEntry> EntriesFor<TCategory>(string userId) =>
        factory.Logs.Entries.Where(e => e.Category == typeof(TCategory).FullName && e.Message.Contains(userId));

    private async Task AssertLoggedAsync<TCategory>(string email, LogLevel level, string text)
    {
        var userId = (await FindUserAsync(factory, email)).Id;
        var entry = Assert.Single(EntriesFor<TCategory>(userId), e => e.Message.Contains(text));
        Assert.Equal(level, entry.Level);
    }

    /// <summary>
    /// Links a GitHub login to the user and returns its provider key. Each key is unique, because
    /// a provider key can belong to only one user and the tests share one identity store.
    /// </summary>
    private async Task<string> AddExternalLoginAsync(string email)
    {
        var providerKey = $"github-{Guid.NewGuid():N}";
        var result = await UseUserManagerAsync(factory, async userManager =>
            await userManager.AddLoginAsync((await userManager.FindByEmailAsync(email))!,
                new UserLoginInfo("GitHub", providerKey, "GitHub")));
        Assert.True(result.Succeeded, string.Join(", ", result.Errors.Select(e => e.Description)));
        return providerKey;
    }

    private Task<IList<UserLoginInfo>> GetLoginsAsync(string email) =>
        UseUserManagerAsync(factory, async userManager =>
            await userManager.GetLoginsAsync((await userManager.FindByEmailAsync(email))!));
}
