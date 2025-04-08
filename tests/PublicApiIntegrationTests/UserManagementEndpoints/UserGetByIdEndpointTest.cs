using System.Net;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class UserGetByIdEndpointTest
{
    [TestMethod]
    public async Task ReturnsItemGivenValidId()
    {
        var client = HttpClientHelper.GetAdminClient();

        var userName = "admin@microsoft.com";
        var response = await client.GetAsync($"api/users/name/{userName}");
        var adminUserResponse = await response.Content.ReadAsStringAsync();
        var adminUser = adminUserResponse.FromJson<GetUserResponse>();
        
        var getUserById = await client.GetAsync($"api/users/{adminUser!.User!.Id}");
        getUserById.EnsureSuccessStatusCode();

        var adminUserByIdResponse = await response.Content.ReadAsStringAsync();
        var adminModel = adminUserByIdResponse.FromJson<GetUserResponse>();
        Assert.IsNotNull(adminModel);
        Assert.IsNotNull(adminModel.User.UserName);
        Assert.AreEqual(userName, adminModel.User.UserName);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidId()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync("api/users/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
