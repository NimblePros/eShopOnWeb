using System.Net;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.RoleManagementEndpoints;

[TestClass]
public class RoleListEndpointTest
{
    [TestMethod]
    public async Task ReturnsUnauthorizedForAnonymousAccess()
    {
        var client = ProgramTest.NewClient;
        var response = await client.GetAsync("/api/roles");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);        
    }


    [TestMethod]
    public async Task ReturnsForbiddenForGeneralAuthorizedAccess()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.GetAsync("/api/roles");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsSuccessAndRolesForAdministratorAccess()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync("/api/roles");
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<RoleListResponse>();
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Roles);
        Assert.IsTrue(model.Roles.Count > 0);
    }
}
