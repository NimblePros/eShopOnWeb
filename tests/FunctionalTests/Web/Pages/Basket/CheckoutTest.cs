using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection("Sequential")]
public class CheckoutTest : IClassFixture<TestApplication>
{
    public CheckoutTest(TestApplication factory)
    {
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    public HttpClient Client { get; }

    [Fact]
    public async Task SucessfullyPay()
    {

        // Load Home Page
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Add Item to Cart
        var keyValues = new List<KeyValuePair<string, string>>
        {
           new("id", "2"),
           new("name", "shirt"),
           new("price", "19.49"),
           new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(stringResponse))
        };
        var formContent = new FormUrlEncodedContent(keyValues);
        var postResponse = await Client.PostAsync("/basket/index", formContent, TestContext.Current.CancellationToken);
        postResponse.EnsureSuccessStatusCode();
        var stringPostResponse = await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(".NET Black &amp; White Mug", stringPostResponse);

        //Load login page
        var loginResponse = await Client.GetAsync("/Identity/Account/Login", TestContext.Current.CancellationToken);
        var longinKeyValues = new List<KeyValuePair<string, string>>
        {
           new("email", "demouser@microsoft.com"),
           new("password", "Pass@word1"),
           new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(await loginResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken)))
        };
        var loginFormContent = new FormUrlEncodedContent(longinKeyValues);
        var loginPostResponse = await Client.PostAsync("/Identity/Account/Login?ReturnUrl=%2FBasket%2FCheckout", loginFormContent, TestContext.Current.CancellationToken);
        var loginStringResponse = await loginPostResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        //Basket checkout (Pay now)
        var checkOutKeyValues = new List<KeyValuePair<string, string>>
        {
           new("Items[0].Id", "2"),
           new("Items[0].Quantity", "1"),
           new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(loginStringResponse))
        };
        var checkOutContent = new FormUrlEncodedContent(checkOutKeyValues);     
        var checkOutResponse = await Client.PostAsync("/basket/checkout", checkOutContent, TestContext.Current.CancellationToken);
        var stringCheckOutResponse = await checkOutResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("/Basket/Success", checkOutResponse.RequestMessage!.RequestUri!.ToString());
        Assert.Contains("Thanks for your Order!", stringCheckOutResponse);
    }
}
