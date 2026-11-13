using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.BasketServiceTests;

public class AddItemToBasket
{
    private readonly string _buyerId = "Test buyerId";
    private readonly IRepository<Basket> _mockBasketRepo = Substitute.For<IRepository<Basket>>();
    private readonly IAppLogger<BasketService> _mockLogger = Substitute.For<IAppLogger<BasketService>>();

    [Fact]
    public async Task InvokesBasketRepositoryGetBySpecAsyncOnce()
    {
        var basket = new Basket(_buyerId);
        basket.AddItem(1, 1.5m);

        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>()).Returns(basket);

        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        await basketService.AddItemToBasket(basket.BuyerId, 1, 1.50m);

        await _mockBasketRepo.Received().FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokesBasketRepositoryUpdateAsyncOnce()
    {
        var basket = new Basket(_buyerId);
        basket.AddItem(1, 1.1m, 1);
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>()).Returns(basket);

        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        await basketService.AddItemToBasket(basket.BuyerId, 1, 1.50m);

        await _mockBasketRepo.Received().UpdateAsync(basket, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreatesAndAddsNewBasketWithItemIfBasketNotFound()
    {
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Basket?)null);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        var result = await basketService.AddItemToBasket(_buyerId, 1, 1.50m, 2);

        Assert.Equal(_buyerId, result.BuyerId);
        var item = Assert.Single(result.Items);
        Assert.Equal(1, item.CatalogItemId);
        Assert.Equal(1.50m, item.UnitPrice);
        Assert.Equal(2, item.Quantity);
        await _mockBasketRepo.Received(1).AddAsync(result, Arg.Any<CancellationToken>());
        await _mockBasketRepo.Received(1).UpdateAsync(result, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DoesNotAddNewBasketIfBasketExists()
    {
        var basket = new Basket(_buyerId);
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        var result = await basketService.AddItemToBasket(_buyerId, 1, 1.50m);

        Assert.Same(basket, result);
        await _mockBasketRepo.DidNotReceive().AddAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }
}
