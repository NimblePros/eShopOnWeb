using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection("Sequential")]
public class IndexTest : IClassFixture<TestApplication>
{
    public IndexTest(TestApplication factory)
    {
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    public HttpClient Client { get; }


    [Fact]
    public async Task OnPostUpdateTo50Successfully()
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
           new("__RequestVerificationToken", token)
        };
        var formContent = new FormUrlEncodedContent(keyValues);
        var postResponse = await Client.PostAsync("/basket/index", formContent, TestContext.Current.CancellationToken);
        postResponse.EnsureSuccessStatusCode();
        var stringResponse = await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(".NET Black &amp; White Mug", stringResponse);

        //Update
        var updateKeyValues = new List<KeyValuePair<string, string>>
        {
           new("Items[0].Id", WebPageHelpers.GetId(stringResponse)),
           new("Items[0].Quantity", "49"),
           new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(stringResponse))
        };
        var updateContent = new FormUrlEncodedContent(updateKeyValues);
        var updateResponse = await Client.PostAsync("/basket/update", updateContent, TestContext.Current.CancellationToken);

        var stringUpdateResponse = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("/basket/update", updateResponse!.RequestMessage!.RequestUri!.ToString()!);
        decimal expectedTotalAmount = 416.50M;
        Assert.Contains(expectedTotalAmount.ToString("N2"), stringUpdateResponse);
    }

    [Fact]
    public async Task OnPostUpdateTo0EmptyBasket()
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
            new("__RequestVerificationToken", token)
        };
        var formContent = new FormUrlEncodedContent(keyValues);
        var postResponse = await Client.PostAsync("/basket/index", formContent, TestContext.Current.CancellationToken);
        postResponse.EnsureSuccessStatusCode();
        var stringResponse = await postResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains(".NET Black &amp; White Mug", stringResponse);

        //Update
        var updateKeyValues = new List<KeyValuePair<string, string>>
        {
            new("Items[0].Id", WebPageHelpers.GetId(stringResponse)),
            new("Items[0].Quantity", "0"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(stringResponse))
        };
        var updateContent = new FormUrlEncodedContent(updateKeyValues);
        var updateResponse = await Client.PostAsync("/basket/update", updateContent, TestContext.Current.CancellationToken);

        var stringUpdateResponse = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        Assert.Contains("/basket/update", updateResponse!.RequestMessage!.RequestUri!.ToString()!);
        Assert.Contains("Basket is empty", stringUpdateResponse);
    }
}
