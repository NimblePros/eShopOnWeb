using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class UpdateUserEndpointTest
{
    [TestMethod]
    public async Task UpdatesSuccessfullyGivenValidChangesAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userName = "demouser@microsoft.com";

        var response = await client.GetAsync($"api/users/name/{userName}");

        var demoUserResponse = await response.Content.ReadAsStringAsync();
        var demoUser = demoUserResponse.FromJson<Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models.GetUserResponse>();

        var startingEmailConfirmedValue = demoUser!.User.EmailConfirmed;
        demoUser.User.EmailConfirmed = !startingEmailConfirmedValue;

        UpdateUserRequest request = new UpdateUserRequest()
        {
            User = demoUser.User
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        var updateResponse = await client.PutAsync("api/users", jsonContent);
        updateResponse.EnsureSuccessStatusCode();
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var updateResponseJsonString = await updateResponse.Content.ReadAsStringAsync();
        var updatedItem = updateResponseJsonString.FromJson<UpdateUserResponse>();
        Assert.IsNotNull(updatedItem);
        Assert.AreEqual(!startingEmailConfirmedValue, updatedItem.User.EmailConfirmed);
    }

    [TestMethod]
    public async Task ReturnsNotFoundForNullRequest()
    {
        var client = HttpClientHelper.GetAdminClient();

        UpdateUserRequest? request = null;
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var updateResponse = await client.PutAsync("api/users", jsonContent);
        Assert.AreEqual(HttpStatusCode.NotFound, updateResponse.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundForNullUserInRequest()
    {
        var client = HttpClientHelper.GetAdminClient();

        UpdateUserRequest request = new();        
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        var updateResponse = await client.PutAsync("api/users", jsonContent);
        Assert.AreEqual(HttpStatusCode.NotFound, updateResponse.StatusCode);
    }
}
