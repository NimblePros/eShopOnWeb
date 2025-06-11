using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection("Sequential")]
public class BasketPageCheckout : IClassFixture<TestApplication>
{
    public BasketPageCheckout(TestApplication factory)
    {
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    public HttpClient Client { get; }

    [Fact]
    public async Task RedirectsToLoginIfNotAuthenticated()
    {

        // Load Home Page
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse1 = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        string token = WebPageHelpers.GetRequestVerificationToken(stringResponse1);

        // Add Item to Cart
        var keyValues = new List<KeyValuePair<string, string>>
        {
            new("id", "2"),
            new("name", "shirt"),
            new("price", "19.49"),
            new("__RequestVerificationToken", token)
        };
        var formContent = new FormUrlEncodedContent(keyValues);
        var postResponse = await Client.PostAsync("/basket/index", formContent, TestContext.Current.CancellationToken);
        postResponse.EnsureSuccessStatusCode();
        var stringResponse = await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(".NET Black &amp; White Mug", stringResponse);

        keyValues.Clear();

        formContent = new FormUrlEncodedContent(keyValues);
        var postResponse2 = await Client.PostAsync("/Basket/Checkout", formContent, TestContext.Current.CancellationToken);
        Assert.Contains("/Identity/Account/Login", postResponse2!.RequestMessage!.RequestUri!.ToString()!);
    }
}
