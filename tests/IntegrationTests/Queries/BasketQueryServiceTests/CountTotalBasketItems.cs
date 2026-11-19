using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.Infrastructure.Data.Queries;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Queries.BasketQueryServiceTests;

public class CountTotalBasketItems
{
    private const string BuyerId = "buyer@eshop.test";
    private readonly CatalogContext _catalogContext;
    private readonly BasketQueryService _queryService;

    public CountTotalBasketItems()
    {
        var dbOptions = new DbContextOptionsBuilder<CatalogContext>()
            .UseInMemoryDatabase(databaseName: $"BasketQuery-{Guid.NewGuid():N}")
            .Options;
        _catalogContext = new CatalogContext(dbOptions);
        _queryService = new BasketQueryService(_catalogContext);
    }

    [Fact]
    public async Task SumsQuantitiesOfAllLinesGivenAllCatalogItemsExist()
    {
        var first = await AddCatalogItemAsync();
        var second = await AddCatalogItemAsync();
        await AddBasketAsync(BuyerId, (first.Id, 2), (second.Id, 3));

        var count = await _queryService.CountTotalBasketItems(BuyerId);

        Assert.Equal(5, count);
    }

    [Fact]
    public async Task ExcludesLinesGivenTheirCatalogItemWasDeleted()
    {
        var kept = await AddCatalogItemAsync();
        var deleted = await AddCatalogItemAsync();
        await AddBasketAsync(BuyerId, (kept.Id, 1), (deleted.Id, 2));
        _catalogContext.CatalogItems.Remove(deleted);
        await _catalogContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var count = await _queryService.CountTotalBasketItems(BuyerId);

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task ReturnsZeroGivenEveryCatalogItemWasDeleted()
    {
        var deleted = await AddCatalogItemAsync();
        await AddBasketAsync(BuyerId, (deleted.Id, 4));
        _catalogContext.CatalogItems.Remove(deleted);
        await _catalogContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var count = await _queryService.CountTotalBasketItems(BuyerId);

        Assert.Equal(0, count);
    }

    [Fact]
    public async Task IgnoresOtherBuyersBaskets()
    {
        var item = await AddCatalogItemAsync();
        await AddBasketAsync(BuyerId, (item.Id, 1));
        await AddBasketAsync("other@eshop.test", (item.Id, 7));

        var count = await _queryService.CountTotalBasketItems(BuyerId);

        Assert.Equal(1, count);
    }

    private async Task<CatalogItem> AddCatalogItemAsync()
    {
        var item = new CatalogItem(catalogTypeId: 1, catalogBrandId: 1, "description",
            $"Item {Guid.NewGuid():N}", 1.00m, "http://catalogbaseurltobereplaced/images/products/1.png");
        _catalogContext.CatalogItems.Add(item);
        await _catalogContext.SaveChangesAsync(TestContext.Current.CancellationToken);
        return item;
    }

    private async Task AddBasketAsync(string buyerId, params (int CatalogItemId, int Quantity)[] lines)
    {
        var basket = new Basket(buyerId);
        foreach (var (catalogItemId, quantity) in lines)
        {
            basket.AddItem(catalogItemId, 1.00m, quantity);
        }
        _catalogContext.Baskets.Add(basket);
        await _catalogContext.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
