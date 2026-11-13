using Ardalis.Result;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Services;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using NSubstitute;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.BasketServiceTests;

public class SetQuantities
{
    private const int TestBasketId = 1;
    private const int TestCatalogItemId = 123;
    private const string TestBuyerId = "Test buyerId";
    private const string UnknownItemKey = "999";

    // Items added through the aggregate have not been persisted, so each has the default Id.
    private static readonly string _itemKey = default(int).ToString();

    private readonly IRepository<Basket> _mockBasketRepo = Substitute.For<IRepository<Basket>>();
    private readonly IAppLogger<BasketService> _mockLogger = Substitute.For<IAppLogger<BasketService>>();

    [Fact]
    public async Task ReturnsNotFoundGivenMissingBasket()
    {
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns((Basket?)null);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        var result = await basketService.SetQuantities(TestBasketId, new Dictionary<string, int>());

        Assert.Equal(ResultStatus.NotFound, result.Status);
        await _mockBasketRepo.DidNotReceive().UpdateAsync(Arg.Any<Basket>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdatesQuantityOfItemWithMatchingId()
    {
        var basket = ArrangeBasketWithOneItem(quantity: 1);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        var result = await basketService.SetQuantities(TestBasketId, new Dictionary<string, int> { [_itemKey] = 5 });

        Assert.True(result.IsSuccess);
        Assert.Same(basket, result.Value);
        Assert.Equal(5, basket.Items.Single().Quantity);
        await _mockBasketRepo.Received(1).UpdateAsync(basket, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LogsQuantityChangeOfItemWithMatchingId()
    {
        ArrangeBasketWithOneItem(quantity: 1);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        await basketService.SetQuantities(TestBasketId, new Dictionary<string, int> { [_itemKey] = 5 });

        _mockLogger.Received(1).LogInformation(Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    public async Task LeavesQuantityUnchangedForItemWithoutMatchingId()
    {
        var basket = ArrangeBasketWithOneItem(quantity: 2);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        await basketService.SetQuantities(TestBasketId, new Dictionary<string, int> { [UnknownItemKey] = 5 });

        Assert.Equal(2, basket.Items.Single().Quantity);
        _mockLogger.DidNotReceive().LogInformation(Arg.Any<string>(), Arg.Any<object[]>());
        await _mockBasketRepo.Received(1).UpdateAsync(basket, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RemovesItemWhoseQuantityIsSetToZero()
    {
        var basket = ArrangeBasketWithOneItem(quantity: 3);
        var basketService = new BasketService(_mockBasketRepo, _mockLogger);

        await basketService.SetQuantities(TestBasketId, new Dictionary<string, int> { [_itemKey] = 0 });

        Assert.Empty(basket.Items);
    }

    [Fact]
    public async Task UpdatesQuantityWhenLoggerIsNull()
    {
        var basket = ArrangeBasketWithOneItem(quantity: 1);
        var basketService = new BasketService(_mockBasketRepo, logger: null!);

        await basketService.SetQuantities(TestBasketId, new Dictionary<string, int> { [_itemKey] = 4 });

        Assert.Equal(4, basket.Items.Single().Quantity);
    }

    private Basket ArrangeBasketWithOneItem(int quantity)
    {
        var basket = new Basket(TestBuyerId);
        basket.AddItem(TestCatalogItemId, 1.50m, quantity);
        _mockBasketRepo.FirstOrDefaultAsync(Arg.Any<BasketWithItemsSpecification>(), Arg.Any<CancellationToken>())
            .Returns(basket);
        return basket;
    }
}
