using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Authorization;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.RoleManagementEndpoints;

[TestClass]
public class CreateRoleEndpointTest
{
    private string _testName = "test role";

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var jsonContent = GetValidNewItemJson();
        var client = HttpClientHelper.GetNormalUserClient();
        var response = await client.PostAsync("api/roles", jsonContent);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsSuccessGivenValidNewItemAndAdminUserToken()
    {
        var jsonContent = GetValidNewItemJson();
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.PostAsync("api/roles", jsonContent);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<CreateRoleResponse>();
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Role);
        Assert.AreEqual(_testName, model.Role.Name);
    }


    [TestMethod]
    public async Task ReturnsConflictForDuplicateRoleName()
    {
        var request = new CreateRoleRequest()
        {
            Name = Constants.Roles.ADMINISTRATORS
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.PostAsync("api/roles", jsonContent);

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    private StringContent GetValidNewItemJson()
    {
        var request = new CreateRoleRequest()
        {
            Name = _testName
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        return jsonContent;
    }
}
