using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BlazorShared.Authorization;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.RoleManagementEndpoints;

[TestClass]
public class RoleGetByIdEndpointTest
{
    [TestMethod]
    public async Task ReturnsItemGivenValidId()
    {
        var client = HttpClientHelper.GetAdminClient();
        var roleList = await client.GetAsync("/api/roles");
        var getAllRolesResponse = await roleList.Content.ReadAsStringAsync();
        var model = getAllRolesResponse.FromJson<RoleListResponse>();
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Roles);
        var adminRole = model.Roles.FirstOrDefault(x => x.Name == Constants.Roles.ADMINISTRATORS);
        Assert.IsNotNull(adminRole);

        var response = await client.GetAsync($"api/roles/{adminRole.Id}");
        response.EnsureSuccessStatusCode();

        var adminRoleResponse = await response.Content.ReadAsStringAsync();
        var adminModel = adminRoleResponse.FromJson<GetByIdRoleResponse>();
        Assert.IsNotNull(adminModel);
        Assert.IsNotNull(adminModel.Role);
        Assert.AreEqual(Constants.Roles.ADMINISTRATORS, adminModel.Role.Name);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidId()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync("api/roles/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
