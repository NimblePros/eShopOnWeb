using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class CustomerOrderWithItemsByIdSpec
{
    private const string OwnerBuyerId = "owner@example.com";
    private const string OtherBuyerId = "other@example.com";
    private readonly Address _shipToAddress = new("Street", "City", "OH", "US", "11111");

    [Fact]
    public void ReturnsOrderWithItemsGivenOwnerAndMatchingId()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrderWithItemsByIdSpec(OwnerBuyerId, 2);

        var result = spec.Evaluate(GetTestCollection()).Single();

        Assert.Equal(2, result.Id);
        Assert.Equal(OwnerBuyerId, result.BuyerId);
        Assert.NotNull(result.OrderItems.Single().ItemOrdered);
    }

    [Fact]
    public void ReturnsNothingGivenAnotherBuyersOrderId()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrderWithItemsByIdSpec(OwnerBuyerId, 3);

        var result = spec.Evaluate(GetTestCollection());

        Assert.Empty(result);
    }

    [Fact]
    public void ReturnsNothingGivenUnknownOrderId()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrderWithItemsByIdSpec(OwnerBuyerId, 99);

        var result = spec.Evaluate(GetTestCollection());

        Assert.Empty(result);
    }

    private List<Order> GetTestCollection() =>
    [
        CreateOrder(1, OwnerBuyerId),
        CreateOrder(2, OwnerBuyerId),
        CreateOrder(3, OtherBuyerId)
    ];

    private Order CreateOrder(int id, string buyerId)
    {
        var order = new Order(buyerId, _shipToAddress,
            [new OrderItem(new CatalogItemOrdered(id, $"Product{id}", "testurl"), 10.50m, 1)]);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(order, id);
        return order;
    }
}
