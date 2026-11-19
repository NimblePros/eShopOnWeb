using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.BasketTests;

public class BasketSetNewBuyerId
{
    [Fact]
    public void ChangesBuyerIdAndKeepsItems()
    {
        var basket = new Basket("anonymous-id");
        basket.AddItem(catalogItemId: 1, unitPrice: 1.00m, quantity: 2);

        basket.SetNewBuyerId("user@example.com");

        Assert.Equal("user@example.com", basket.BuyerId);
        Assert.Equal(2, Assert.Single(basket.Items).Quantity);
    }
}
