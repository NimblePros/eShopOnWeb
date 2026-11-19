using Ardalis.Result;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.BasketServiceTests;

public class RemoveItemsForCatalogItems
{
    private const int TestBasketId = 1;
    private const string TestBuyerId = "Test buyerId";

    private readonly IRepository<Basket> _mockBasketRepo = Substitute.For<IRepository<Basket>>();
    private readonly IAppLogger<BasketService> _mockLogger = Substitute.For<IAppLogger<BasketService>>();

    [Fact]
    public async Task ReturnsNotFoundGivenMissingBasket()
    {
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Basket?)null);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        var result = await basketService.RemoveItemsForCatalogItemsAsync(TestBasketId, [1]);

        Assert.Equal(ResultStatus.NotFound, result.Status);
        await _mockBasketRepo.DidNotReceive().UpdateAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemovesMatchingItemsAndSavesBasket()
    {
        var basket = new Basket(TestBuyerId);
        basket.AddItem(1, 1.5m, 1);
        basket.AddItem(2, 2.5m, 1);
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        var result = await basketService.RemoveItemsForCatalogItemsAsync(TestBasketId, [1]);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, Assert.Single(result.Value.Items).CatalogItemId);
        await _mockBasketRepo.Received(1).UpdateAsync(basket, Arg.Any<CancellationToken>());
    }
}
