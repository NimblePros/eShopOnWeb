using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.BasketTests;

public class BasketItemSetQuantity
{
    private static BasketItem CreateItem(int quantity = 1) => new(catalogItemId: 1, quantity, unitPrice: 2.50m);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(BasketItem.MaxQuantity)]
    public void SetsQuantityGivenValueWithinRange(int quantity)
    {
        var item = CreateItem();

        item.SetQuantity(quantity);

        Assert.Equal(quantity, item.Quantity);
    }

    [Fact]
    public void ThrowsGivenNegativeQuantity()
    {
        var item = CreateItem(quantity: 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.SetQuantity(-1));
        Assert.Equal(3, item.Quantity);
    }

    [Theory]
    [InlineData(BasketItem.MaxQuantity + 1)]
    [InlineData(int.MaxValue)]
    public void ThrowsGivenQuantityAboveMax(int quantity)
    {
        var item = CreateItem(quantity: 3);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.SetQuantity(quantity));
        Assert.Equal(3, item.Quantity);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(BasketItem.MaxQuantity + 1)]
    public void ConstructorThrowsGivenQuantityOutOfRange(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateItem(quantity));
    }

    [Fact]
    public void AddQuantityAddsToExistingQuantity()
    {
        var item = CreateItem(quantity: 2);

        item.AddQuantity(3);

        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void AddQuantityThrowsGivenNegativeQuantity()
    {
        var item = CreateItem(quantity: 2);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.AddQuantity(-1));
        Assert.Equal(2, item.Quantity);
    }

    [Theory]
    [InlineData(BasketItem.MaxQuantity)]
    [InlineData(int.MaxValue)]
    public void AddQuantityThrowsGivenResultAboveMax(int quantity)
    {
        var item = CreateItem(quantity: 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.AddQuantity(quantity));
        Assert.Equal(1, item.Quantity);
    }

    [Fact]
    public void AddQuantityAllowsResultOfExactlyMax()
    {
        var item = CreateItem(quantity: 1);

        item.AddQuantity(BasketItem.MaxQuantity - 1);

        Assert.Equal(BasketItem.MaxQuantity, item.Quantity);
    }

    [Theory]
    [InlineData(2, 3, 5)]
    [InlineData(1, BasketItem.MaxQuantity - 1, BasketItem.MaxQuantity)]
    [InlineData(1, BasketItem.MaxQuantity, BasketItem.MaxQuantity)]
    [InlineData(BasketItem.MaxQuantity, 1, BasketItem.MaxQuantity)]
    [InlineData(500, int.MaxValue, BasketItem.MaxQuantity)]
    public void AddQuantityUpToMaxCapsResultAtMax(int initial, int added, int expected)
    {
        var item = CreateItem(initial);

        item.AddQuantityUpToMax(added);

        Assert.Equal(expected, item.Quantity);
    }

    [Fact]
    public void AddQuantityUpToMaxThrowsGivenNegativeQuantity()
    {
        var item = CreateItem(quantity: 2);

        Assert.Throws<ArgumentException>(() => item.AddQuantityUpToMax(-1));
        Assert.Equal(2, item.Quantity);
    }
}
