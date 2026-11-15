using Microsoft.eShopWeb.ApplicationCore.Constants;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Constants;

public class DemoUserConstantsTests
{
    [Theory]
    [InlineData(BlazorShared.Authorization.Constants.DemoUsers.DEMO_USER, "demouser")]
    [InlineData(BlazorShared.Authorization.Constants.DemoUsers.PRODUCT_MANAGER, "productmgr")]
    [InlineData(BlazorShared.Authorization.Constants.DemoUsers.ADMINISTRATOR, "admin")]
    public void DemoUsersUseReservedEshopTestDomain(string email, string expectedLocalPart)
    {
        Assert.Equal($"{expectedLocalPart}@eshop.test", email);
    }

    [Fact]
    public void DefaultAdminUserNameIsSeededAdministrator()
    {
        Assert.Equal(BlazorShared.Authorization.Constants.DemoUsers.ADMINISTRATOR, AuthorizationConstants.DEFAULT_ADMIN_USERNAME);
    }
}
