using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class CustomerOrdersWithItemsSpecification
{
    private const string OtherBuyerId = "OtherBuyerId";
    private readonly string _buyerId = "TestBuyerId";
    private Address _shipToAddress = new Address("Street", "City", "OH", "US", "11111");

    [Fact]
    public void ReturnsOrderWithOrderedItem()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrdersWithItemsSpecification(_buyerId);

        var result = spec.Evaluate(GetTestCollection()).FirstOrDefault();

        Assert.NotNull(result);
        Assert.NotNull(result.OrderItems);
        Assert.Single(result.OrderItems);
        Assert.NotNull(result.OrderItems.FirstOrDefault()?.ItemOrdered);
    }

    [Fact]
    public void ReturnsAllOrderWithAllOrderedItem()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrdersWithItemsSpecification(_buyerId);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Single(result[0].OrderItems);
        Assert.NotNull(result[0].OrderItems.FirstOrDefault()?.ItemOrdered);
        Assert.Equal(2, result[1].OrderItems.Count);
        Assert.NotNull(result[1].OrderItems.ToList()[0].ItemOrdered);
        Assert.NotNull(result[1].OrderItems.ToList()[1].ItemOrdered);
    }

    [Fact]
    public void ExcludesOtherBuyersOrders()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrdersWithItemsSpecification(_buyerId);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.All(result, order => Assert.Equal(_buyerId, order.BuyerId));
    }

    [Fact]
    public void ReturnsNothingGivenBuyerWithNoOrders()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CustomerOrdersWithItemsSpecification("buyer-without-orders");

        Assert.Empty(spec.Evaluate(GetTestCollection()));
    }

    public List<Order> GetTestCollection()
    {
        var ordersList = new List<Order>();

        ordersList.Add(new Order(_buyerId, _shipToAddress,
            new List<OrderItem>
            {
                    new OrderItem(new CatalogItemOrdered(1, "Product1", "testurl"), 10.50m, 1)
            }));
        ordersList.Add(new Order(_buyerId, _shipToAddress,
            new List<OrderItem>
            {
                    new OrderItem(new CatalogItemOrdered(2, "Product2", "testurl"), 15.50m, 2),
                    new OrderItem(new CatalogItemOrdered(2, "Product3", "testurl"), 20.50m, 1)
            }));
        ordersList.Add(new Order(OtherBuyerId, _shipToAddress,
            new List<OrderItem>
            {
                    new OrderItem(new CatalogItemOrdered(3, "Product4", "testurl"), 5.00m, 1)
            }));

        return ordersList;
    }
}
