using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.BasketTests;

public class BasketItemSetQuantity
{
    private static BasketItem CreateItem(int quantity = 1) => new(catalogItemId: 1, quantity, unitPrice: 2.50m);

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(int.MaxValue)]
    public void SetsQuantityGivenNonNegativeValue(int quantity)
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

    [Fact]
    public void ConstructorThrowsGivenNegativeQuantity()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateItem(quantity: -1));
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

    [Fact]
    public void AddQuantityThrowsGivenResultThatWouldOverflow()
    {
        var item = CreateItem(quantity: 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => item.AddQuantity(int.MaxValue));
        Assert.Equal(1, item.Quantity);
    }

    [Fact]
    public void AddQuantityAllowsResultOfExactlyIntMaxValue()
    {
        var item = CreateItem(quantity: 1);

        item.AddQuantity(int.MaxValue - 1);

        Assert.Equal(int.MaxValue, item.Quantity);
    }
}
