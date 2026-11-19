using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.UnitTests.Builders;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.OrderTests;

public class OrderConstructor
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsGivenMissingBuyerId(string? buyerId)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Order(buyerId!, new AddressBuilder().Build(), []));
    }

    [Fact]
    public void StampsOrderDateWhenCreated()
    {
        var before = DateTimeOffset.Now;

        var order = new OrderBuilder().Build();

        Assert.InRange(order.OrderDate, before, DateTimeOffset.Now);
    }

    [Theory]
    [InlineData(0, "Product", "uri")]
    [InlineData(1, "", "uri")]
    [InlineData(1, "Product", "")]
    public void CatalogItemOrderedThrowsGivenInvalidSnapshot(int catalogItemId, string productName, string pictureUri)
    {
        Assert.ThrowsAny<ArgumentException>(() => new CatalogItemOrdered(catalogItemId, productName, pictureUri));
    }
}
