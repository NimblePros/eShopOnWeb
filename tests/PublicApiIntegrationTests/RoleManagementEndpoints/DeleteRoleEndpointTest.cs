using BlazorAdmin.Models;
using BlazorAdmin.Pages.CatalogItemPage;
using BlazorShared.Authorization;
using BlazorShared.Models;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PublicApiIntegrationTests.RoleManagementEndpoints;

[TestClass]
public class DeleteRoleEndpointTest
{
    [TestMethod]
    public async Task ReturnsSuccessGivenValidIdAndAdminUserToken()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var response = await client.DeleteAsync("api/roles/12");
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<DeleteCatalogItemResponse>();
        Assert.IsNotNull(model);
        Assert.AreEqual("Deleted", model!.Status);
    }

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
        var productManager = model.Roles.FirstOrDefault(x => x.Name == Constants.Roles.PRODUCT_MANAGERS);
        Assert.IsNotNull(productManager);
        // Try to delete it with it already assigned

        var response = await client.DeleteAsync($"api/roles/{productManager.Id}");

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }
}
