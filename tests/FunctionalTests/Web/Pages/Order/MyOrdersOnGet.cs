using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Order;

[Collection("Sequential")]
public class MyOrdersOnGet(TestApplication factory) : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory = factory;

    private HttpClient CreateClient(bool allowAutoRedirect = false) =>
        _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = allowAutoRedirect });

    [Fact]
    public async Task ReturnsRedirectGivenAnonymousUser()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/order/my-orders", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task ReturnsOrderHistoryGivenAuthenticatedUser()
    {
        var client = CreateClient();
        await WebPageHelpers.LoginAsync(client);

        var response = await client.GetAsync("/order/my-orders", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("My Order History", content);
    }

    [Fact]
    public async Task ListsPlacedOrderWithWorkingDetailLink()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = CreateClient(allowAutoRedirect: true);
        await WebPageHelpers.LoginAsync(client);

        // Add an item to the signed-in user's basket and check out.
        var home = await client.GetStringAsync("/", ct);
        var addResponse = await client.PostAsync("/basket/index", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("id", "2"),
            new("name", "shirt"),
            new("price", "19.49"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(home))
        }), ct);
        addResponse.EnsureSuccessStatusCode();
        var basketPage = await addResponse.Content.ReadAsStringAsync(ct);

        var checkoutResponse = await client.PostAsync("/basket/checkout", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>
        {
            new("Items[0].Id", WebPageHelpers.GetId(basketPage)),
            new("Items[0].Quantity", "1"),
            new(WebPageHelpers.TokenTag, WebPageHelpers.GetRequestVerificationToken(basketPage))
        }), ct);
        Assert.Contains("Thanks for your Order!", await checkoutResponse.Content.ReadAsStringAsync(ct));

        var myOrders = await client.GetStringAsync("/order/my-orders", ct);
        var detailLink = Regex.Match(myOrders, @"href=""(/order/detail/\d+)""");
        Assert.True(detailLink.Success, "Expected a link to the order detail page.");

        var detailResponse = await client.GetAsync(detailLink.Groups[1].Value, ct);

        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadAsStringAsync(ct);
        Assert.Contains("ORDER DETAILS", detail);
        Assert.Contains(".NET Black &amp; White Mug", detail);
    }

    [Fact]
    public async Task ReturnsBadRequestGivenUnknownOrder()
    {
        var client = CreateClient();
        await WebPageHelpers.LoginAsync(client);

        var response = await client.GetAsync("/order/detail/999999", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task DetailReturnsRedirectGivenAnonymousUser()
    {
        var client = CreateClient();

        var response = await client.GetAsync("/order/detail/1", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location!.OriginalString);
    }
}
