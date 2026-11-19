using System.Net;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Microsoft.eShopWeb.FunctionalTests.Web.WebPageHelpers;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.Basket;

/// <summary>
/// A shopper's basket can still hold a line for a product that was deleted after it was added.
/// </summary>
[Collection(WebTestCollection.Name)]
public class DeletedCatalogItemTest(TestApplication factory)
{
    private const string UnavailableItemsMessage = "no longer available";

    [Fact]
    public async Task BasketPageHidesLineGivenItsCatalogItemWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient(allowAutoRedirect: true);
        var (catalogItemId, productName) = await AddCatalogItemAsync();
        await AddItemToBasketAsync(client, catalogItemId);
        await AddItemToBasketAsync(client);
        await DeleteCatalogItemAsync(catalogItemId);

        var response = await client.GetAsync("/basket", ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var basketPage = await response.Content.ReadAsStringAsync(ct);
        Assert.DoesNotContain(productName, basketPage);
        Assert.Contains(MugProductNameHtml, basketPage);
    }

    [Fact]
    public async Task BasketBadgeCountExcludesLineGivenItsCatalogItemWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var client = factory.CreateClient(allowAutoRedirect: true);
        var (catalogItemId, _) = await AddCatalogItemAsync();
        await AddItemToBasketAsync(client, catalogItemId);
        await AddItemToBasketAsync(client, catalogItemId);
        await AddItemToBasketAsync(client);
        Assert.Equal("3", GetBasketBadgeCount(await client.GetStringAsync("/basket", ct)));
        await DeleteCatalogItemAsync(catalogItemId);

        var basketPage = await client.GetStringAsync("/basket", ct);

        Assert.Equal("1", GetBasketBadgeCount(basketPage));
        Assert.Equal("1", GetFirstBasketItemQuantity(basketPage));
    }

    [Fact]
    public async Task CheckoutRemovesDeletedItemAndRedirectsToBasketWithoutPlacingOrder()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(allowAutoRedirect: true);
        await LoginAsync(client, email);
        var (catalogItemId, _) = await AddCatalogItemAsync();
        await AddItemToBasketAsync(client, catalogItemId);
        var basketPage = await AddItemToBasketAsync(client);
        await DeleteCatalogItemAsync(catalogItemId);

        var response = await PostFormAsync(client, "/basket/checkout", basketPage);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("/Basket", response.RequestMessage!.RequestUri!.AbsolutePath);
        var redirectedPage = await response.Content.ReadAsStringAsync(ct);
        Assert.Contains(UnavailableItemsMessage, redirectedPage);
        Assert.Contains(MugProductNameHtml, redirectedPage);
        Assert.DoesNotContain("href=\"/order/detail/", await client.GetStringAsync("/order/my-orders", ct));
        Assert.Equal(0, await CountBasketLinesAsync(email, catalogItemId));
    }

    [Fact]
    public async Task CheckoutSucceedsAfterDeletedItemWasRemoved()
    {
        var ct = TestContext.Current.CancellationToken;
        var email = await CreateUserAsync(factory);
        var client = factory.CreateClient(allowAutoRedirect: true);
        await LoginAsync(client, email);
        var (catalogItemId, _) = await AddCatalogItemAsync();
        await AddItemToBasketAsync(client, catalogItemId);
        var basketPage = await AddItemToBasketAsync(client);
        await DeleteCatalogItemAsync(catalogItemId);
        var redirectedResponse = await PostFormAsync(client, "/basket/checkout", basketPage);
        var redirectedPage = await redirectedResponse.Content.ReadAsStringAsync(ct);

        var response = await PostFormAsync(client, "/basket/checkout", redirectedPage,
            new("Items[0].Id", GetId(redirectedPage)),
            new("Items[0].Quantity", "1"));

        Assert.Contains("Thanks for your Order!", await response.Content.ReadAsStringAsync(ct));
    }

    private async Task<(int Id, string Name)> AddCatalogItemAsync()
    {
        var name = $"Soon Deleted {Guid.NewGuid():N}";
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
        var item = new CatalogItem(catalogTypeId: 1, catalogBrandId: 1, "Deleted after being added to a basket",
            name, 1.50m, "http://catalogbaseurltobereplaced/images/products/1.png");
        context.CatalogItems.Add(item);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        return (item.Id, name);
    }

    private async Task DeleteCatalogItemAsync(int catalogItemId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
        var item = await context.CatalogItems.FindAsync([catalogItemId], TestContext.Current.CancellationToken);
        context.CatalogItems.Remove(item!);
        await context.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    private async Task<int> CountBasketLinesAsync(string buyerId, int catalogItemId)
    {
        using var scope = factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<CatalogContext>();
        return context.Baskets
            .Where(b => b.BuyerId == buyerId)
            .SelectMany(b => b.Items)
            .Count(i => i.CatalogItemId == catalogItemId);
    }
}
