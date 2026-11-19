using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection(WebTestCollection.Name)]
public class CheckoutTest(TestApplication factory)
{
    [Fact]
    public async Task SucessfullyPay()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient(allowAutoRedirect: true);
        await AddItemToBasketAsync(client);

        // Signing in transfers the anonymous basket to the user, then returns to checkout.
        var loginPage = await client.GetStringAsync("/Identity/Account/Login", ct);
        var checkoutPageResponse = await PostFormAsync(client, "/Identity/Account/Login?ReturnUrl=%2FBasket%2FCheckout", loginPage,
            new("email", DemoUserEmail),
            new("password", DemoUserPassword));
        var checkoutPage = await checkoutPageResponse.Content.ReadAsStringAsync(ct);
        Assert.Contains(MugProductNameHtml, checkoutPage);

        var checkOutResponse = await PostFormAsync(client, "/basket/checkout", checkoutPage,
            new("Items[0].Id", GetId(checkoutPage)),
            new("Items[0].Quantity", "1"));

        Assert.Contains("/Basket/Success", checkOutResponse.RequestMessage!.RequestUri!.ToString());
        Assert.Contains("Thanks for your Order!", await checkOutResponse.Content.ReadAsStringAsync(ct));
    }

    [Fact]
    public async Task EmptiesBasketAfterSuccessfulCheckout()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(allowAutoRedirect: true);
        await LoginAsync(client, email);
        var basketPage = await AddItemToBasketAsync(client);

        var checkOutResponse = await PostFormAsync(client, "/basket/checkout", basketPage,
            new("Items[0].Id", GetId(basketPage)),
            new("Items[0].Quantity", "1"));
        Assert.Contains("Thanks for your Order!", await checkOutResponse.Content.ReadAsStringAsync(ct));

        Assert.Contains("Basket is empty", await client.GetStringAsync("/basket", ct));
    }

    [Fact]
    public async Task RedirectsToBasketWithoutPlacingOrderGivenEmptyBasket()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(allowAutoRedirect: true);
        await LoginAsync(client, email);
        var checkoutPage = await client.GetStringAsync("/basket/checkout", ct);

        var response = await PostFormAsync(client, "/basket/checkout", checkoutPage);

        Assert.Equal("/Basket", response.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("Basket is empty", await response.Content.ReadAsStringAsync(ct));
        Assert.DoesNotContain("href=\"/order/detail/", await client.GetStringAsync("/order/my-orders", ct));
    }
}
