using Microsoft.eShopWeb.ApplicationCore.Entities.BuyerAggregate;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.BuyerTests;

public class BuyerConstructor
{
    [Fact]
    public void SetsIdentityAndStartsWithNoPaymentMethods()
    {
        var buyer = new Buyer("identity-guid");

        Assert.Equal("identity-guid", buyer.IdentityGuid);
        Assert.Empty(buyer.PaymentMethods);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void ThrowsGivenMissingIdentity(string? identity)
    {
        Assert.ThrowsAny<ArgumentException>(() => new Buyer(identity!));
    }
}
