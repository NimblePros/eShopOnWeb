using System.Net;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class UserGetByUserNameEndpointTest
{
    [TestMethod]
    [DataRow("admin@microsoft.com")]
    [DataRow("productmgr@microsoft.com")]
    [DataRow("demouser@microsoft.com")]
    public async Task ReturnsItemGivenValidName(string userName)
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync($"api/users/name/{userName}");
        response.EnsureSuccessStatusCode();

        var adminRoleResponse = await response.Content.ReadAsStringAsync();
        var adminModel = adminRoleResponse.FromJson<GetUserResponse>();
        Assert.IsNotNull(adminModel);
        Assert.IsNotNull(adminModel.User);
        Assert.IsNotNull(adminModel.User.UserName);
        Assert.AreEqual(userName, adminModel.User.UserName);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidName()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync("api/users/name/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
