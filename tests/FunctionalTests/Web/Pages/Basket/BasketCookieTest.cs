using System.Net;
using Microsoft.eShopWeb.Web;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection(WebTestCollection.Name)]
public class BasketCookieTest(TestApplication factory)
{
    [Fact]
    public async Task IssuesHttpOnlySecureStrictCookieForAnonymousBasket()
    {
        var client = factory.CreateClient(allowAutoRedirect: false);

        var response = await client.GetAsync("/basket", TestContext.Current.CancellationToken);

        var setCookie = GetBasketSetCookie(response);
        Assert.NotNull(setCookie);
        var attributes = setCookie.Split(';', StringSplitOptions.TrimEntries);
        Assert.True(Guid.TryParse(attributes[0][(Constants.BASKET_COOKIENAME.Length + 1)..], out _), setCookie);
        Assert.Contains("httponly", attributes, StringComparer.OrdinalIgnoreCase);
        Assert.Contains("secure", attributes, StringComparer.OrdinalIgnoreCase);
        // The cookie policy's MinimumSameSitePolicy raises the cookie's own Lax to Strict.
        Assert.Contains("samesite=strict", attributes, StringComparer.OrdinalIgnoreCase);
        Assert.Contains(attributes, a => a.StartsWith("expires=", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task IgnoresBasketCookieThatNamesAnotherUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var victimEmail = await CreateVictimWithBasketItemAsync();
        var attacker = factory.CreateClient(allowAutoRedirect: false);

        var response = await attacker.SendAsync(WithBasketCookie(HttpMethod.Get, "/basket", victimEmail), ct);

        response.EnsureSuccessStatusCode();
        var page = await response.Content.ReadAsStringAsync(ct);
        Assert.Contains("Basket is empty", page);
        Assert.DoesNotContain(MugProductNameHtml, page);
        // The forged value is replaced with a fresh anonymous id.
        Assert.NotNull(GetBasketSetCookie(response));
    }

    [Fact]
    public async Task CheckoutRedirectsToLoginGivenBasketCookieThatNamesAnotherUser()
    {
        var ct = TestContext.Current.CancellationToken;
        var victimEmail = await CreateVictimWithBasketItemAsync();
        var attacker = factory.CreateClient(allowAutoRedirect: false);

        var response = await attacker.SendAsync(WithBasketCookie(HttpMethod.Get, "/Basket/Checkout", victimEmail), ct);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }

    private async Task<string> CreateVictimWithBasketItemAsync()
    {
        var email = await CreateUserAsync(factory);
        var victim = factory.CreateClient(allowAutoRedirect: true);
        await LoginAsync(victim, email);
        Assert.Contains(MugProductNameHtml, await AddItemToBasketAsync(victim));
        return email;
    }

    private static HttpRequestMessage WithBasketCookie(HttpMethod method, string url, string value)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Add("Cookie", $"{Constants.BASKET_COOKIENAME}={value}");
        return request;
    }

    private static string? GetBasketSetCookie(HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out var values)
            ? values.FirstOrDefault(v => v.StartsWith($"{Constants.BASKET_COOKIENAME}=", StringComparison.Ordinal))
            : null;
}
