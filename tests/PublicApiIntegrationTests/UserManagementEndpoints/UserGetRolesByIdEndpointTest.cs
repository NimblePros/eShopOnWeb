using System.Net;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class UserGetRolesByIdEndpointTest
{
    [TestMethod]
    public async Task ReturnsItemGivenValidId()
    {
        var client = HttpClientHelper.GetAdminClient();

        var userName = "admin@microsoft.com";
        var response = await client.GetAsync($"api/users/name/{userName}");
        var adminUserResponse = await response.Content.ReadAsStringAsync();
        var adminUser = adminUserResponse.FromJson<GetUserResponse>();
        
        var getUserRoles = await client.GetAsync($"api/users/{adminUser!.User!.Id}/roles");
        getUserRoles.EnsureSuccessStatusCode();

        var userRoles = await getUserRoles.Content.ReadAsStringAsync();
        var userRolesList = userRoles.FromJson<GetUserRolesResponse>();
        Assert.IsNotNull(userRolesList);
        Assert.IsNotNull(userRolesList.Roles);
        Assert.IsTrue(userRolesList.Roles.Count > 0);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidId()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync("api/users/0/roles");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
