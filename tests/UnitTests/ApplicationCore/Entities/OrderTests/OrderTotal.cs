using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.UnitTests.Builders;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.OrderTests;

public class OrderTotal
{
    private readonly decimal _testUnitPrice = 42m;

    [Fact]
    public void IsZeroForNewOrder()
    {
        var order = new OrderBuilder().WithNoItems();

        Assert.Equal(0, order.Total());
    }

    [Fact]
    public void IsCorrectGiven1Item()
    {
        var builder = new OrderBuilder();
        var items = new List<OrderItem>
            {
                new(builder.CreateCatalogItemOrdered(), _testUnitPrice, 1)
            };
        var order = new OrderBuilder().WithItems(items);
        Assert.Equal(_testUnitPrice, order.Total());
    }

    [Fact]
    public void SumsEachLineUnitPriceTimesUnits()
    {
        var builder = new OrderBuilder();
        var order = builder.WithItems(
        [
            new(builder.CreateCatalogItemOrdered(), 2.50m, 2),
            new(builder.CreateCatalogItemOrdered(), 10.00m, 1),
            new(builder.CreateCatalogItemOrdered(), 0.99m, 3)
        ]);

        Assert.Equal(17.97m, order.Total());
    }

    [Fact]
    public void IsCorrectGiven3Items()
    {
        var builder = new OrderBuilder();
        var order = builder.WithDefaultValues();

        Assert.Equal(builder.TestUnitPrice * builder.TestUnits, order.Total());
    }
}
