using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.UnitTests.Builders;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class OrderWithItemsByIdSpec
{
    [Fact]
    public void ReturnsOrderWithItemsGivenMatchingId()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.OrderWithItemsByIdSpec(2);

        var result = Assert.Single(spec.Evaluate(GetTestCollection()));

        Assert.Equal(2, result.Id);
        Assert.NotNull(Assert.Single(result.OrderItems).ItemOrdered);
    }

    [Fact]
    public void ReturnsOrderRegardlessOfBuyer()
    {
        // Unlike CustomerOrderWithItemsByIdSpec, this spec does no ownership check.
        var spec = new eShopWeb.ApplicationCore.Specifications.OrderWithItemsByIdSpec(3);

        var result = Assert.Single(spec.Evaluate(GetTestCollection()));

        Assert.Equal("other@example.com", result.BuyerId);
    }

    [Fact]
    public void ReturnsNothingGivenUnknownId()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.OrderWithItemsByIdSpec(99);

        Assert.Empty(spec.Evaluate(GetTestCollection()));
    }

    private static List<Order> GetTestCollection() =>
    [
        new OrderBuilder().WithBuyer("buyer@example.com").WithId(1),
        new OrderBuilder().WithBuyer("buyer@example.com").WithId(2),
        new OrderBuilder().WithBuyer("other@example.com").WithId(3)
    ];
}
