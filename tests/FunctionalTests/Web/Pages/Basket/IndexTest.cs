using System.Text.RegularExpressions;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.Web.Pages.Basket;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

[Collection(WebTestCollection.Name)]
public class IndexTest(TestApplication factory)
{
    // Each test gets its own client, so its own anonymous basket cookie.
    private readonly HttpClient _client = factory.CreateClient(allowAutoRedirect: true);

    [Fact]
    public async Task AddsItemToBasketGivenCatalogItemId()
    {
        var basketPage = await AddItemToBasketAsync(_client);

        Assert.Contains(MugProductNameHtml, basketPage);
        Assert.Equal("1", GetFirstBasketItemQuantity(basketPage));
    }

    [Fact]
    public async Task IncrementsQuantityGivenSameItemAddedTwice()
    {
        await AddItemToBasketAsync(_client);

        var basketPage = await AddItemToBasketAsync(_client);

        Assert.Single(Regex.Matches(basketPage, @"name=""Items\[\d+\]\.Id"""));
        Assert.Equal("2", GetFirstBasketItemQuantity(basketPage));
    }

    [Fact]
    public async Task RedirectsHomeWithoutAddingGivenUnknownCatalogItemId()
    {
        var ct = TestContext.Current.CancellationToken;
        var home = await _client.GetStringAsync("/", ct);

        var response = await PostFormAsync(_client, "/basket/index", home, KeyValuePair.Create("id", "999999"));

        response.EnsureSuccessStatusCode();
        Assert.Equal("/", response.RequestMessage!.RequestUri!.AbsolutePath);
        Assert.Contains("Basket is empty", await _client.GetStringAsync("/basket", ct));
    }

    [Fact]
    public async Task OnPostUpdateTo50Successfully()
    {
        var basketPage = await AddItemToBasketAsync(_client);

        var updateResponse = await UpdateFirstBasketItemQuantityAsync(_client, basketPage, 49);

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        Assert.Contains("/basket/update", updateResponse.RequestMessage!.RequestUri!.ToString());
        Assert.Contains((MugUnitPrice * 49).ToString("N2"), updatedPage);
    }

    [Fact]
    public async Task OnPostUpdateTo0EmptyBasket()
    {
        var basketPage = await AddItemToBasketAsync(_client);

        var updateResponse = await UpdateFirstBasketItemQuantityAsync(_client, basketPage, 0);

        Assert.Contains("/basket/update", updateResponse.RequestMessage!.RequestUri!.ToString());
        Assert.Contains("Basket is empty", await updateResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task OnPostUpdateKeepsQuantityGivenNegativeQuantity()
    {
        var ct = TestContext.Current.CancellationToken;
        var basketPage = await AddItemToBasketAsync(_client);

        await UpdateFirstBasketItemQuantityAsync(_client, basketPage, -1);

        var reloaded = await _client.GetStringAsync("/basket", ct);
        Assert.Contains(MugProductNameHtml, reloaded);
        Assert.Equal("1", GetFirstBasketItemQuantity(reloaded));
    }

    [Fact]
    public async Task OnPostUpdateToMaxQuantitySucceeds()
    {
        var ct = TestContext.Current.CancellationToken;
        var basketPage = await AddItemToBasketAsync(_client);

        await UpdateFirstBasketItemQuantityAsync(_client, basketPage, BasketItem.MaxQuantity);

        Assert.Equal(BasketItem.MaxQuantity.ToString(), GetFirstBasketItemQuantity(await _client.GetStringAsync("/basket", ct)));
    }

    [Fact]
    public async Task OnPostUpdateKeepsQuantityAndShowsErrorGivenQuantityAboveMax()
    {
        var ct = TestContext.Current.CancellationToken;
        var basketPage = await AddItemToBasketAsync(_client);

        var updateResponse = await UpdateFirstBasketItemQuantityAsync(_client, basketPage, BasketItem.MaxQuantity + 1);

        var updatedPage = await updateResponse.Content.ReadAsStringAsync(ct);
        Assert.Contains($"Quantity must be between 0 and {BasketItem.MaxQuantity}.", updatedPage);
        // The basket is still rendered alongside the error, not replaced by an empty one.
        Assert.Contains(MugProductNameHtml, updatedPage);
        Assert.Equal("1", GetFirstBasketItemQuantity(await _client.GetStringAsync("/basket", ct)));
    }

    [Fact]
    public async Task AddingItemAlreadyAtMaxQuantityShowsMessageAndKeepsQuantity()
    {
        var basketPage = await AddItemToBasketAsync(_client);
        await UpdateFirstBasketItemQuantityAsync(_client, basketPage, BasketItem.MaxQuantity);

        var afterAdd = await AddItemToBasketAsync(_client);

        Assert.Contains(IndexModel.MaxQuantityReachedMessage, afterAdd);
        Assert.Equal(BasketItem.MaxQuantity.ToString(), GetFirstBasketItemQuantity(afterAdd));
    }

    [Fact]
    public async Task OnPostUpdateLeavesAnotherBasketUntouchedGivenItsItemId()
    {
        var ct = TestContext.Current.CancellationToken;
        var victimPage = await AddItemToBasketAsync(_client);
        var attacker = factory.CreateClient(allowAutoRedirect: true);
        var attackerPage = await AddItemToBasketAsync(attacker);

        // Post the victim's basket item id from the attacker's basket form.
        await PostFormAsync(attacker, "/basket/update", attackerPage,
            new("Items[0].Id", GetId(victimPage)),
            new("Items[0].Quantity", "0"));

        Assert.Equal("1", GetFirstBasketItemQuantity(await _client.GetStringAsync("/basket", ct)));
    }
}
