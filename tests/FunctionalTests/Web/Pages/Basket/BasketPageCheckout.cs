using System.Net;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection(WebTestCollection.Name)]
public class BasketPageCheckout(TestApplication factory)
{
    [Fact]
    public async Task RedirectsToLoginIfNotAuthenticated()
    {
        var client = factory.CreateClient(allowAutoRedirect: true);
        await AddItemToBasketAsync(client);

        var response = await client.PostAsync("/Basket/Checkout", new FormUrlEncodedContent([]),
            TestContext.Current.CancellationToken);

        Assert.Contains("/Identity/Account/Login", response.RequestMessage!.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetRedirectsToLoginIfNotAuthenticated()
    {
        var client = factory.CreateClient(allowAutoRedirect: false);

        var response = await client.GetAsync("/Basket/Checkout", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }
}
