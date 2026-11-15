using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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
    public async Task ReturnsUnauthorizedGivenNoToken()
    {
        var client = ProgramTest.NewClient;

        var response = await client.PutAsync("api/users", JsonContent.Create(UpdateRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.PutAsync("api/users", JsonContent.Create(UpdateRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();

        var response = await client.PutAsync("api/users", JsonContent.Create(UpdateRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenNonExistentUserIdAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.PutAsync("api/users", JsonContent.Create(UpdateRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsUpdatedUserGivenValidUserAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        var request = UpdateRequest(userId);

        var response = await client.PutAsync("api/users", JsonContent.Create(request));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var model = (await response.Content.ReadAsStringAsync()).FromJson<UpdateUserResponse>();
        Assert.IsNotNull(model?.User);
        Assert.AreEqual(userId, model.User.Id);
        Assert.AreEqual(request.User.UserName, model.User.UserName);
        Assert.AreEqual(request.User.Email, model.User.Email);
        Assert.AreEqual(request.User.PhoneNumber, model.User.PhoneNumber);
    }

    [TestMethod]
    public async Task DoesNotExposeSensitiveIdentityFieldsGivenValidUserAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);

        var response = await client.PutAsync("api/users", JsonContent.Create(UpdateRequest(userId)));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.IsTrue(json.RootElement.TryGetProperty("user", out var user));
        UserJsonAssert.HasOnlyUserDtoFields(user);
    }

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
        Assert.AreEqual(HttpStatusCode.OK, updateResponse.StatusCode);

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

    private static UpdateUserRequest UpdateRequest(string userId)
    {
        var userName = $"{Guid.NewGuid()}@example.com";
        return new UpdateUserRequest
        {
            User = new UserDto { Id = userId, UserName = userName, Email = userName, PhoneNumber = "555-0100" }
        };
    }

    // Creates a dedicated user so updates don't affect the seeded users other tests rely on.
    private static async Task<string> CreateUserAsync(HttpClient client)
    {
        var createRequest = new CreateUserRequest { User = new UserDto { UserName = $"{Guid.NewGuid()}@example.com" } };
        var response = await client.PostAsync("api/users", JsonContent.Create(createRequest));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<CreateUserResponse>()!.UserId;
    }
}
