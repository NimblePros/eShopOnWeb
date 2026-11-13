using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Extensions;

public class BasketGuardsEmptyBasketOnCheckout
{
    private const string TestBuyerId = "Test buyerId";

    [Fact]
    public void ThrowsGivenBasketWithNoItems()
    {
        var basket = new Basket(TestBuyerId);

        Assert.Throws<EmptyBasketOnCheckoutException>(() => Guard.Against.EmptyBasketOnCheckout(basket.Items));
    }

    [Fact]
    public void DoesNotThrowGivenBasketWithItems()
    {
        var basket = new Basket(TestBuyerId);
        basket.AddItem(catalogItemId: 1, unitPrice: 1.00m);

        var exception = Record.Exception(() => Guard.Against.EmptyBasketOnCheckout(basket.Items));

        Assert.Null(exception);
    }
}
