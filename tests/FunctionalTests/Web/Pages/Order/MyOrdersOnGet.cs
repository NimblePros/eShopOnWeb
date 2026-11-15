using System.Net;
using System.Text.RegularExpressions;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Order;

[Collection("Sequential")]
public class MyOrdersOnGet(TestApplication factory) : IClassFixture<TestApplication>
{
    private readonly TestApplication _factory = factory;

    private HttpClient CreateClient(bool allowAutoRedirect = false) =>
        _factory.CreateClient(allowAutoRedirect: allowAutoRedirect);

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

        var detailUrl = await PlaceOrderAndGetDetailUrlAsync(client);

        var detailResponse = await client.GetAsync(detailUrl, ct);

        detailResponse.EnsureSuccessStatusCode();
        var detail = await detailResponse.Content.ReadAsStringAsync(ct);
        Assert.Contains("ORDER DETAILS", detail);
        Assert.Contains(".NET Black &amp; White Mug", detail);
    }

    [Fact]
    public async Task DetailReturnsBadRequestGivenAnotherUsersOrder()
    {
        var ct = TestContext.Current.CancellationToken;

        // The demo user places an order.
        var owner = CreateClient(allowAutoRedirect: true);
        await WebPageHelpers.LoginAsync(owner);
        var detailUrl = await PlaceOrderAndGetDetailUrlAsync(owner);
        (await owner.GetAsync(detailUrl, ct)).EnsureSuccessStatusCode();

        // A different user asks for the same order.
        var otherEmail = await WebPageHelpers.CreateUserAsync(_factory);
        var other = CreateClient();
        await WebPageHelpers.LoginAsync(other, otherEmail);

        var response = await other.GetAsync(detailUrl, ct);

        // Same response as for an order that doesn't exist, so the order's existence isn't revealed.
        var unknownResponse = await other.GetAsync("/order/detail/999999", ct);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(await unknownResponse.Content.ReadAsStringAsync(ct), await response.Content.ReadAsStringAsync(ct));
    }

    private static async Task<string> PlaceOrderAndGetDetailUrlAsync(HttpClient client)
    {
        var ct = TestContext.Current.CancellationToken;
        var home = await client.GetStringAsync("/", ct);
        var addResponse = await WebPageHelpers.PostFormAsync(client, "/basket/index", home,
            new("id", "2"), new("name", "shirt"), new("price", "19.49"));
        addResponse.EnsureSuccessStatusCode();
        var basketPage = await addResponse.Content.ReadAsStringAsync(ct);

        var checkoutResponse = await WebPageHelpers.PostFormAsync(client, "/basket/checkout", basketPage,
            new("Items[0].Id", WebPageHelpers.GetId(basketPage)), new("Items[0].Quantity", "1"));
        Assert.Contains("Thanks for your Order!", await checkoutResponse.Content.ReadAsStringAsync(ct));

        // Order ids are identity values, so the highest one is the order just placed.
        var myOrders = await client.GetStringAsync("/order/my-orders", ct);
        var orderIds = Regex.Matches(myOrders, @"href=""/order/detail/(\d+)""")
            .Select(m => int.Parse(m.Groups[1].Value))
            .ToList();
        Assert.NotEmpty(orderIds);
        return $"/order/detail/{orderIds.Max()}";
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
