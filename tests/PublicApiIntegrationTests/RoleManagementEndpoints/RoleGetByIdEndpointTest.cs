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
public class RoleGetByIdEndpointTest
{
    [TestMethod]
    public async Task ReturnsItemGivenValidId()
    {
        var token = ApiTokenHelper.GetAdminUserToken();

        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var roleList = await client.GetAsync("/api/roles");
        var stringResponse = await roleList.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<RoleListResponse>();
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Roles);
        var adminRole = model.Roles.FirstOrDefault(x => x.Name == Constants.Roles.ADMINISTRATORS);
        Assert.IsNotNull(adminRole);

        var response = await client.GetAsync($"api/roles/{adminRole.Id}");
        response.EnsureSuccessStatusCode();
        
        var adminStringResponse = await response.Content.ReadAsStringAsync();
        var adminModel = adminStringResponse.FromJson<GetByIdRoleResponse>();
        Assert.IsNotNull(adminModel);
        Assert.IsNotNull(adminModel.Role);
        Assert.AreEqual(Constants.Roles.ADMINISTRATORS, adminModel.Role.Name);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidId()
    {
        var token = ApiTokenHelper.GetAdminUserToken();

        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync("api/roles/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
