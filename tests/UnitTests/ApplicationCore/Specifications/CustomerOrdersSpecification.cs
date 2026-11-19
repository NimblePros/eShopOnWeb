using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.UnitTests.Builders;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class CustomerOrdersSpecification
{
    private const string BuyerId = "buyer@example.com";
    private const string OtherBuyerId = "other@example.com";

    [Fact]
    public void ReturnsOnlyTheBuyersOrders()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrdersSpecification(BuyerId);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal([1, 2], result.Select(o => o.Id));
    }

    [Fact]
    public void ReturnsNothingGivenBuyerWithNoOrders()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrdersSpecification("nobody@example.com");

        Assert.Empty(spec.Evaluate(GetTestCollection()));
    }

    private static List<Order> GetTestCollection() =>
    [
        new OrderBuilder().WithBuyer(BuyerId).WithId(1),
        new OrderBuilder().WithBuyer(BuyerId).WithId(2),
        new OrderBuilder().WithBuyer(OtherBuyerId).WithId(3)
    ];
}
