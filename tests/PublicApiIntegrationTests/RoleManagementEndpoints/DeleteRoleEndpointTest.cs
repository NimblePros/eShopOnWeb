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
public class DeleteRoleEndpointTest
{
    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidIdAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.DeleteAsync("api/roles/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsConflictWhenDeletingAnAssignedRole()
    {
        var client = HttpClientHelper.GetAdminClient();

        // Get the role id for Product Manager
        var roleList = await client.GetAsync("/api/roles");
        var stringResponse = await roleList.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<RoleListResponse>();
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Roles);
        var administrator = model.Roles.FirstOrDefault(x => x.Name == Constants.Roles.ADMINISTRATORS);
        Assert.IsNotNull(administrator);
        // Try to delete it with it already assigned

        var response = await client.DeleteAsync($"api/roles/{administrator.Id}");

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
}
