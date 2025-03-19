using System.Net;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class DeleteUserEndpointTest
{
    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidIdAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.DeleteAsync("api/users/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenProductManagerToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();

        var response = await client.DeleteAsync("api/users/0");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.DeleteAsync("api/users/0");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenTryingToDeleteAdminUser()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userName = "admin@microsoft.com";
        var response = await client.GetAsync($"api/users/name/{userName}");

        var adminUserResponse = await response.Content.ReadAsStringAsync();
        var adminUser = adminUserResponse.FromJson<Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models.GetUserResponse>();
        var deleteResponse = await client.DeleteAsync($"api/users/{adminUser!.User!.Id}");

        Assert.AreEqual(HttpStatusCode.Forbidden, deleteResponse.StatusCode);

    }
}
