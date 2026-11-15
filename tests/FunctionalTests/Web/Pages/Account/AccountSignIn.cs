using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Account;

[Collection("Sequential")]
public class AccountSignIn : IClassFixture<TestApplication>
{
    public AccountSignIn(TestApplication factory)
    {
        Client = factory.CreateClient(allowAutoRedirect: false);
    }

    public HttpClient Client { get; }

    [Fact]
    public async Task ReturnsSignInScreenOnGet()
    {
        var response = await Client.GetAsync("/identity/account/login", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("demouser@eshop.test", stringResponse);
    }

    [Fact]
    public void RegexMatchesValidRequestVerificationToken()
    {
        // TODO: Move to a unit test
        // TODO: Move regex to a constant in test project
        var input = @"<input name=""__RequestVerificationToken"" type=""hidden"" value=""CfDJ8Obhlq65OzlDkoBvsSX0tgxFUkIZ_qDDSt49D_StnYwphIyXO4zxfjopCWsygfOkngsL6P0tPmS2HTB1oYW-p_JzE0_MCFb7tF9Ol_qoOg_IC_yTjBNChF0qRgoZPmKYOIJigg7e2rsBsmMZDTdbnGo"" /><input name=""RememberMe"" type=""hidden"" value=""false"" /></form>";
        string regexpression = @"name=""__RequestVerificationToken"" type=""hidden"" value=""([-A-Za-z0-9+=/\\_]+?)""";
        var regex = new Regex(regexpression);
        var match = regex.Match(input);
        var group = match.Groups.Values.LastOrDefault();
        Assert.NotNull(group);
        Assert.True(group.Value.Length > 50);
    }

    [Fact]
    public async Task ReturnsFormWithRequestVerificationToken()
    {
        var response = await Client.GetAsync("/identity/account/login", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        string token = WebPageHelpers.GetRequestVerificationToken(stringResponse);
        Assert.True(token.Length > 50);
    }

    [Fact]
    public async Task ReturnsSuccessfulSignInOnPostWithValidCredentials()
    {
        var getResponse = await Client.GetAsync("/identity/account/login", TestContext.Current.CancellationToken);
        getResponse.EnsureSuccessStatusCode();
        var stringResponse1 = await getResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        var keyValues = new List<KeyValuePair<string, string>>
        {
            new("Email", "demouser@eshop.test"),
            new("Password", "Pass@word1"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(stringResponse1))
        };
        var formContent = new FormUrlEncodedContent(keyValues);

        var postResponse = await Client.PostAsync("/identity/account/login", formContent, TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal(new System.Uri("/", UriKind.Relative), postResponse.Headers.Location);
    }

    [Fact]
    public async Task IssuesHttpOnlySecureStrictAuthCookieOnSignIn()
    {
        var postResponse = await WebPageHelpers.LoginAsync(Client);
        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);

        Assert.True(postResponse.Headers.TryGetValues("Set-Cookie", out var setCookies));
        var authCookie = Assert.Single(setCookies, c => c.StartsWith("EshopIdentifier=", StringComparison.Ordinal));
        var attributes = authCookie.Split(';').Skip(1).Select(a => a.Trim().ToLowerInvariant()).ToList();
        Assert.Contains("httponly", attributes);
        Assert.Contains("secure", attributes);
        Assert.Contains("samesite=strict", attributes);
    }
}
