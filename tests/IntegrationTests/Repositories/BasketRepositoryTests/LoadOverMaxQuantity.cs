using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Infrastructure.Data;
using Xunit;

namespace Microsoft.eShopWeb.IntegrationTests.Repositories.BasketRepositoryTests;

public class LoadOverMaxQuantity
{
    private const string BuyerId = "legacy-buyer";
    private const int LegacyQuantity = BasketItem.MaxQuantity + 4001;

    private readonly DbContextOptions<CatalogContext> _dbOptions = new DbContextOptionsBuilder<CatalogContext>()
        .UseInMemoryDatabase(databaseName: $"LoadOverMaxQuantity-{Guid.NewGuid()}")
        .Options;

    private const int CatalogItemId = 1;

    [Fact]
    public async Task LoadsBasketItemSavedWithQuantityAboveMax()
    {
        var basket = await SeedAndLoadLegacyBasketAsync(LegacyQuantity);

        Assert.Equal(LegacyQuantity, basket.Items.Single().Quantity);
    }

    [Fact]
    public async Task AddingToItemAboveMaxThrowsOutOfRangeAndKeepsQuantity()
    {
        var basket = await SeedAndLoadLegacyBasketAsync(LegacyQuantity);

        Assert.Throws<ArgumentOutOfRangeException>(() => basket.AddItem(CatalogItemId, 2.50m, 1));
        Assert.Equal(LegacyQuantity, basket.Items.Single().Quantity);
    }

    [Fact]
    public async Task MergingIntoItemNearIntMaxValueCapsAtMaxWithoutOverflowing()
    {
        var basket = await SeedAndLoadLegacyBasketAsync(int.MaxValue);

        basket.MergeItem(CatalogItemId, 2.50m, 5);

        Assert.Equal(BasketItem.MaxQuantity, basket.Items.Single().Quantity);
    }

    // Rows saved before BasketItem.MaxQuantity existed may hold larger quantities.
    // Write one by going around the domain model's guards, then load it in a fresh context.
    private async Task<Basket> SeedAndLoadLegacyBasketAsync(int legacyQuantity)
    {
        var ct = TestContext.Current.CancellationToken;
        await using (var seedContext = new CatalogContext(_dbOptions))
        {
            var seeded = new Basket(BuyerId);
            seeded.AddItem(CatalogItemId, unitPrice: 2.50m, quantity: 1);
            seedContext.Baskets.Add(seeded);
            await seedContext.SaveChangesAsync(ct);

            seedContext.Entry(seeded.Items.Single()).Property(i => i.Quantity).CurrentValue = legacyQuantity;
            await seedContext.SaveChangesAsync(ct);
        }

        var context = new CatalogContext(_dbOptions);
        var repository = new EfRepository<Basket>(context);
        var basket = await repository.FirstOrDefaultAsync(new BasketWithItemsSpecification(BuyerId), ct);

        Assert.NotNull(basket);
        return basket;
    }
}
