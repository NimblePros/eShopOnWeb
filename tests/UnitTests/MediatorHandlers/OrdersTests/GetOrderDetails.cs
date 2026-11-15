using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Features.OrderDetails;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.MediatorHandlers.OrdersTests;

public class GetOrderDetails
{
    private const string OwnerUserName = "owner@example.com";
    private const int OrderId = 7;
    private readonly IReadRepository<Order> _mockOrderRepository = Substitute.For<IReadRepository<Order>>();

    public GetOrderDetails()
    {
        var item = new OrderItem(new CatalogItemOrdered(1, "ProductName", "URI"), 10.00m, 10);
        var address = new Address("", "", "", "", "");
        var order = new Order(OwnerUserName, address, [item]);
        typeof(BaseEntity).GetProperty(nameof(BaseEntity.Id))!.SetValue(order, OrderId);
        var orders = new List<Order> { order };

        // Apply the handler's specification to an in-memory store, so the test exercises the
        // real filtering rather than a canned result.
        _mockOrderRepository.FirstOrDefaultAsync(Arg.Any<ISpecification<Order>>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<ISpecification<Order>>().Evaluate(orders).FirstOrDefault());
    }

    [Fact]
    public async Task ReturnsOrderDetailsGivenOrderOwner()
    {
        var request = new eShopWeb.Web.Features.OrderDetails.GetOrderDetails(OwnerUserName, OrderId);
        var handler = new GetOrderDetailsHandler(_mockOrderRepository);

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.NotNull(result);
        Assert.Equal(OrderId, result.OrderNumber);
        Assert.Equal(100.00m, result.Total);
        Assert.Equal("ProductName", Assert.Single(result.OrderItems).ProductName);
    }

    [Fact]
    public async Task ReturnsNullGivenAnotherUsersOrder()
    {
        var request = new eShopWeb.Web.Features.OrderDetails.GetOrderDetails("someone-else@example.com", OrderId);
        var handler = new GetOrderDetailsHandler(_mockOrderRepository);

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }

    [Fact]
    public async Task ReturnsNullGivenUnknownOrder()
    {
        var request = new eShopWeb.Web.Features.OrderDetails.GetOrderDetails(OwnerUserName, OrderId + 1);
        var handler = new GetOrderDetailsHandler(_mockOrderRepository);

        var result = await handler.Handle(request, TestContext.Current.CancellationToken);

        Assert.Null(result);
    }
}
