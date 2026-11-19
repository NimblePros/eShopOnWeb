using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.BasketTests;

public class BasketMergeItem
{
    private const int TestCatalogItemId = 123;
    private const decimal TestUnitPrice = 1.23m;
    private readonly Basket _basket = new("Test buyerId");

    [Fact]
    public void AddsBasketItemIfNotPresent()
    {
        _basket.MergeItem(TestCatalogItemId, TestUnitPrice, 4);

        var item = _basket.Items.Single();
        Assert.Equal(TestCatalogItemId, item.CatalogItemId);
        Assert.Equal(TestUnitPrice, item.UnitPrice);
        Assert.Equal(4, item.Quantity);
    }

    [Fact]
    public void IncrementsQuantityOfItemIfPresent()
    {
        _basket.AddItem(TestCatalogItemId, TestUnitPrice, 2);

        _basket.MergeItem(TestCatalogItemId, TestUnitPrice, 3);

        Assert.Equal(5, _basket.Items.Single().Quantity);
    }

    [Fact]
    public void CapsMergedQuantityAtMax()
    {
        _basket.AddItem(TestCatalogItemId, TestUnitPrice, 600);

        _basket.MergeItem(TestCatalogItemId, TestUnitPrice, 600);

        Assert.Equal(BasketItem.MaxQuantity, _basket.Items.Single().Quantity);
    }

    [Fact]
    public void CapsNewItemQuantityAtMax()
    {
        _basket.MergeItem(TestCatalogItemId, TestUnitPrice, int.MaxValue);

        Assert.Equal(BasketItem.MaxQuantity, _basket.Items.Single().Quantity);
    }

    [Fact]
    public void ThrowsGivenNegativeQuantity()
    {
        _basket.AddItem(TestCatalogItemId, TestUnitPrice, 2);

        Assert.Throws<ArgumentException>(() => _basket.MergeItem(TestCatalogItemId, TestUnitPrice, -1));
        Assert.Equal(2, _basket.Items.Single().Quantity);
    }

    [Fact]
    public void DoesNotAddItemGivenNegativeQuantity()
    {
        Assert.Throws<ArgumentException>(() => _basket.MergeItem(TestCatalogItemId, TestUnitPrice, -1));
        Assert.Empty(_basket.Items);
    }
}
