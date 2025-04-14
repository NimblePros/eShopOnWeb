using System.Net;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class UserListEndpointTest
{
    [TestMethod]
    public async Task ReturnsUnauthorizedForAnonymousAccess()
    {
        var client = ProgramTest.NewClient;
        var response = await client.GetAsync("/api/users");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenForGeneralAuthorizedAccess()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.GetAsync("/api/users");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenForProductManagerAccess()
    {
        var client = HttpClientHelper.GetProductManagerClient();

        var response = await client.GetAsync("/api/users");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsSuccessAndRolesForAdministratorAccess()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync("/api/users");
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<UserListResponse>();
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Users);
        Assert.IsTrue(model.Users.Count > 0);
    }
}
