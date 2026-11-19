using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.BasketTests;

public class BasketRemoveItemsForCatalogItems
{
    private const decimal TestUnitPrice = 1.23m;
    private const string TestBuyerId = "Test buyerId";

    [Fact]
    public void RemovesOnlyItemsForTheGivenCatalogItems()
    {
        var basket = new Basket(TestBuyerId);
        basket.AddItem(1, TestUnitPrice, 1);
        basket.AddItem(2, TestUnitPrice, 2);
        basket.AddItem(3, TestUnitPrice, 3);

        basket.RemoveItemsForCatalogItems([1, 3]);

        var remaining = Assert.Single(basket.Items);
        Assert.Equal(2, remaining.CatalogItemId);
        Assert.Equal(2, remaining.Quantity);
    }

    [Fact]
    public void LeavesBasketUnchangedGivenNoMatchingCatalogItems()
    {
        var basket = new Basket(TestBuyerId);
        basket.AddItem(1, TestUnitPrice, 1);

        basket.RemoveItemsForCatalogItems([99]);

        Assert.Equal(1, Assert.Single(basket.Items).CatalogItemId);
    }
}
