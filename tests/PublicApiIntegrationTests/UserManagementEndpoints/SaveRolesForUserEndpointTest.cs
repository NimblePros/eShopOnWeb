using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorShared.Authorization;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class SaveRolesForUserEndpointTest
{
    [TestMethod]
    public async Task ReturnsUnauthorizedGivenNoToken()
    {
        var client = ProgramTest.NewClient;

        var response = await client.PutAsync(RolesRoute("0"), JsonContent.Create(AddProductManagerRequest()));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.PutAsync(RolesRoute("0"), JsonContent.Create(AddProductManagerRequest()));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();

        var response = await client.PutAsync(RolesRoute("0"), JsonContent.Create(AddProductManagerRequest()));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenNonExistentUserIdAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.PutAsync(RolesRoute(Guid.NewGuid().ToString()), JsonContent.Create(AddProductManagerRequest()));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsBadRequestGivenDefaultAdminUser()
    {
        var client = HttpClientHelper.GetAdminClient();
        var adminId = await GetUserIdByNameAsync(client, "admin@microsoft.com");
        var request = new SaveRolesForUserRequest { RolesToRemove = [Constants.Roles.ADMINISTRATORS] };

        var response = await client.PutAsync(RolesRoute(adminId), JsonContent.Create(request));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        CollectionAssert.Contains(await GetRolesAsync(client, adminId), Constants.Roles.ADMINISTRATORS);
    }

    [TestMethod]
    public async Task AddsAndRemovesRolesGivenValidUserAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);

        var addResponse = await client.PutAsync(RolesRoute(userId), JsonContent.Create(AddProductManagerRequest()));

        Assert.AreEqual(HttpStatusCode.OK, addResponse.StatusCode);
        CollectionAssert.AreEquivalent(new[] { Constants.Roles.PRODUCT_MANAGERS }, await GetRolesAsync(client, userId));

        var removeRequest = new SaveRolesForUserRequest { RolesToRemove = [Constants.Roles.PRODUCT_MANAGERS] };
        var removeResponse = await client.PutAsync(RolesRoute(userId), JsonContent.Create(removeRequest));

        Assert.AreEqual(HttpStatusCode.OK, removeResponse.StatusCode);
        Assert.AreEqual(0, (await GetRolesAsync(client, userId)).Count);
    }

    private static string RolesRoute(string userId) => $"api/users/{userId}/roles";

    private static SaveRolesForUserRequest AddProductManagerRequest() =>
        new() { RolesToAdd = [Constants.Roles.PRODUCT_MANAGERS] };

    private static async Task<List<string>> GetRolesAsync(HttpClient client, string userId)
    {
        var response = await client.GetAsync(RolesRoute(userId));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<GetUserRolesResponse>()!.Roles;
    }

    private static async Task<string> GetUserIdByNameAsync(HttpClient client, string userName)
    {
        var response = await client.GetAsync($"api/users/name/{userName}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<GetUserResponse>()!.User.Id;
    }

    // Creates a dedicated user so role changes don't affect the seeded users other tests rely on.
    private static async Task<string> CreateUserAsync(HttpClient client)
    {
        var createRequest = new CreateUserRequest { User = new UserDto { UserName = $"{Guid.NewGuid()}@example.com" } };
        var response = await client.PostAsync("api/users", JsonContent.Create(createRequest));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<CreateUserResponse>()!.UserId;
    }
}
