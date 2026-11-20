using System.Net;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Account;

[Collection(WebTestCollection.Name)]
public class AccountSignIn(TestApplication factory)
{
    private const string AuthCookieName = "EshopIdentifier";

    public HttpClient Client { get; } = factory.CreateClient(allowAutoRedirect: false);

    [Fact]
    public async Task ReturnsSignInScreenOnGet()
    {
        var response = await Client.GetAsync("/identity/account/login", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains(DemoUserEmail, stringResponse);
    }

    [Fact]
    public void GetRequestVerificationTokenExtractsTokenFromHiddenInput()
    {
        const string token = "CfDJ8Obhlq65OzlDkoBvsSX0tgxFUkIZ_qDDSt49D_StnYwphIyXO4zxfjopCWsygfOkngsL6P0tPmS2HTB1oYW-p_JzE0_MCFb7tF9Ol_qoOg_IC_yTjBNChF0qRgoZPmKYOIJigg7e2rsBsmMZDTdbnGo";
        var input = $@"<input name=""__RequestVerificationToken"" type=""hidden"" value=""{token}"" /><input name=""RememberMe"" type=""hidden"" value=""false"" /></form>";

        Assert.Equal(token, GetRequestVerificationToken(input));
    }

    [Fact]
    public async Task ReturnsFormWithRequestVerificationToken()
    {
        var response = await Client.GetAsync("/identity/account/login", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        string token = GetRequestVerificationToken(stringResponse);
        Assert.True(token.Length > 50);
    }

    [Fact]
    public async Task ReturnsSuccessfulSignInOnPostWithValidCredentials()
    {
        var postResponse = await LoginAsync(Client);

        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);
        Assert.Equal(new Uri("/", UriKind.Relative), postResponse.Headers.Location);
    }

    [Theory]
    [InlineData(DemoUserEmail, "not-the-password")]
    [InlineData("nobody@eshop.test", DemoUserPassword)]
    public async Task ShowsErrorWithoutSigningInGivenInvalidCredentials(string email, string password)
    {
        var postResponse = await LoginAsync(Client, email, password);

        Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
        Assert.Contains("Invalid login attempt.", await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        Assert.False(IssuesAuthCookie(postResponse));
    }

    [Fact]
    public async Task RejectsPostWithoutAntiforgeryToken()
    {
        var response = await Client.PostAsync("/identity/account/login", new FormUrlEncodedContent(
        [
            new("Email", DemoUserEmail),
            new("Password", DemoUserPassword)
        ]), TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(IssuesAuthCookie(response));
    }

    [Fact]
    public async Task IssuesHttpOnlySecureStrictAuthCookieOnSignIn()
    {
        var postResponse = await LoginAsync(Client);
        Assert.Equal(HttpStatusCode.Redirect, postResponse.StatusCode);

        Assert.True(postResponse.Headers.TryGetValues("Set-Cookie", out var setCookies));
        var authCookie = Assert.Single(setCookies, c => c.StartsWith($"{AuthCookieName}=", StringComparison.Ordinal));
        var attributes = authCookie.Split(';').Skip(1).Select(a => a.Trim().ToLowerInvariant()).ToList();
        Assert.Contains("httponly", attributes);
        Assert.Contains("secure", attributes);
        Assert.Contains("samesite=strict", attributes);
    }

    [Fact]
    public async Task IssuesStrictAntiforgeryCookieOnSignInPage()
    {
        var response = await Client.GetAsync("/identity/account/login", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();

        // The cookie policy's minimum is Lax, so Strict here comes from the antiforgery cookie's own default.
        Assert.True(response.Headers.TryGetValues("Set-Cookie", out var setCookies));
        var antiforgeryCookie = Assert.Single(setCookies, c => c.StartsWith(".AspNetCore.Antiforgery.", StringComparison.Ordinal));
        var attributes = antiforgeryCookie.Split(';').Skip(1).Select(a => a.Trim().ToLowerInvariant()).ToList();
        Assert.Contains("samesite=strict", attributes);
    }

    private static bool IssuesAuthCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var setCookies)
        && setCookies.Any(c => c.StartsWith($"{AuthCookieName}=", StringComparison.Ordinal));
}
