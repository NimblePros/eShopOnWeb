using Mediator;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.UnitTests.Builders;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.OrderServiceTests;

public class CreateOrderAsync
{
    private const int TestBasketId = 1;
    private const string TestBuyerId = "Test buyerId";
    private const int TestCatalogItemId = 42;
    private const string TestProductName = "Test Product";
    private const string TestPictureUri = "http://catalogbaseurltobereplaced/images/products/1.png";
    private const string ComposedPictureUri = "https://cdn.test/images/products/1.png";
    private const decimal TestUnitPrice = 2.50m;
    private const int TestQuantity = 3;

    private readonly IRepository<Basket> _mockBasketRepo = Substitute.For<IRepository<Basket>>();
    private readonly IRepository<CatalogItem> _mockItemRepo = Substitute.For<IRepository<CatalogItem>>();
    private readonly IRepository<Order> _mockOrderRepo = Substitute.For<IRepository<Order>>();
    private readonly IUriComposer _mockUriComposer = Substitute.For<IUriComposer>();
    private readonly IMediator _mockMediator = Substitute.For<IMediator>();
    private readonly Address _shippingAddress = new AddressBuilder().Build();

    [Fact]
    public async Task ThrowsGivenMissingBasket()
    {
        ArrangeBasket(null);
        var orderService = CreateOrderService();

        await Assert.ThrowsAsync<ArgumentNullException>(() => orderService.CreateOrderAsync(TestBasketId, _shippingAddress));

        await _mockOrderRepo.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ThrowsGivenEmptyBasket()
    {
        ArrangeBasket(new Basket(TestBuyerId));
        var orderService = CreateOrderService();

        await Assert.ThrowsAsync<EmptyBasketOnCheckoutException>(() => orderService.CreateOrderAsync(TestBasketId, _shippingAddress));

        await _mockOrderRepo.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AddsOrderBuiltFromBasketItemsAndCatalogDetails()
    {
        ArrangeBasketWithOneItemAndMatchingCatalogItem();
        Order? addedOrder = null;
        await _mockOrderRepo.AddAsync(Arg.Do<Order>(order => addedOrder = order), Arg.Any<CancellationToken>());

        await CreateOrderService().CreateOrderAsync(TestBasketId, _shippingAddress);

        Assert.NotNull(addedOrder);
        Assert.Equal(TestBuyerId, addedOrder.BuyerId);
        Assert.Same(_shippingAddress, addedOrder.ShipToAddress);
        var orderItem = Assert.Single(addedOrder.OrderItems);
        Assert.Equal(TestCatalogItemId, orderItem.ItemOrdered.CatalogItemId);
        Assert.Equal(TestProductName, orderItem.ItemOrdered.ProductName);
        Assert.Equal(ComposedPictureUri, orderItem.ItemOrdered.PictureUri);
        Assert.Equal(TestUnitPrice, orderItem.UnitPrice);
        Assert.Equal(TestQuantity, orderItem.Units);
    }

    [Fact]
    public async Task PublishesOrderCreatedEventForAddedOrder()
    {
        ArrangeBasketWithOneItemAndMatchingCatalogItem();
        Order? addedOrder = null;
        await _mockOrderRepo.AddAsync(Arg.Do<Order>(order => addedOrder = order), Arg.Any<CancellationToken>());

        await CreateOrderService().CreateOrderAsync(TestBasketId, _shippingAddress);

        Assert.NotNull(addedOrder);
        await _mockMediator.Received(1).Publish(
            Arg.Is<OrderCreatedEvent>(e => e.Order == addedOrder),
            Arg.Any<CancellationToken>());
    }

    private OrderService CreateOrderService() =>
        new(_mockBasketRepo, _mockItemRepo, _mockOrderRepo, _mockUriComposer, _mockMediator);

    private void ArrangeBasket(Basket? basket) =>
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);

    private void ArrangeBasketWithOneItemAndMatchingCatalogItem()
    {
        var basket = new Basket(TestBuyerId);
        basket.AddItem(TestCatalogItemId, TestUnitPrice, TestQuantity);
        ArrangeBasket(basket);

        // Id has a protected setter, so substitute the entity to give it a persisted-looking Id.
        var catalogItem = Substitute.For<CatalogItem>(1, 1, "Description", TestProductName, 9.99m, TestPictureUri);
        catalogItem.Id.Returns(TestCatalogItemId);
        _mockItemRepo.ListAsync(Arg.Any<CatalogItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns([catalogItem]);

        _mockUriComposer.ComposePicUri(TestPictureUri).Returns(ComposedPictureUri);
    }
}
