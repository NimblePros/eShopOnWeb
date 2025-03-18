using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using BlazorShared.Authorization;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.RoleManagementEndpoints;

[TestClass]
public class DeleteRoleEndpointTest
{
    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidIdAndAdminUserToken()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.DeleteAsync("api/roles/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsConflictWhenDeletingAnAssignedRole()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

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
