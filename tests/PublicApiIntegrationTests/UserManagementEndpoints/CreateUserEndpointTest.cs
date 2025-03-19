using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class CreateUserEndpointTest
{
    private string _testName = "test@microsoft.com";

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var jsonContent = GetValidNewItemJson(_testName);
        var client = HttpClientHelper.GetNormalUserClient();
        var response = await client.PostAsync("api/users", jsonContent);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsSuccessGivenValidNewItemAndAdminUserToken()
    {
        var jsonContent = GetValidNewItemJson(_testName);
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.PostAsync("api/users", jsonContent);
        response.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<CreateUserResponse>();
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.User.UserName);
        Assert.AreEqual(_testName, model.User.UserName);
    }


    [TestMethod]
    public async Task ReturnsConflictForDuplicateUserName()
    {
        var jsonContent = GetValidNewItemJson("admin@microsoft.com");
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.PostAsync("api/users", jsonContent);

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    private StringContent GetValidNewItemJson(string userName)
    {
        var newUser = new ApplicationUser()
        {
            UserName = userName
        };
        var request = new CreateUserRequest();
        request.User = newUser;
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        return jsonContent;
    }
}
