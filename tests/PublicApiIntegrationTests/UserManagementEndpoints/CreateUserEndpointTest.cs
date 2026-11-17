using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.UserManagementEndpoints;

[TestClass]
public class CreateUserEndpointTest
{
    private readonly string _testName = "test@eshop.test";

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
        Assert.IsTrue(!string.IsNullOrEmpty(model.UserId));
    }


    [TestMethod]
    public async Task ReturnsConflictForDuplicateUserName()
    {
        var jsonContent = GetValidNewItemJson("admin@eshop.test");
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.PostAsync("api/users", jsonContent);

        Assert.AreEqual(HttpStatusCode.Conflict, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsBadRequestForMissingUserName()
    {
        var jsonContent = GetInvalidNewItemJson();
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.PostAsync("api/users", jsonContent);

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    public async Task IgnoresConfirmationFlagsInRequest()
    {
        var client = HttpClientHelper.GetAdminClient();
        var newUser = new UserDto
        {
            UserName = $"{Guid.NewGuid()}@example.com",
            PhoneNumber = "555-0100",
            EmailConfirmed = true,
            PhoneNumberConfirmed = true
        };
        newUser.Email = newUser.UserName;

        var userId = await CreateUserAsync(client, newUser);

        var stored = await GetUserByIdAsync(client, userId);
        Assert.IsFalse(stored.EmailConfirmed);
        Assert.IsFalse(stored.PhoneNumberConfirmed);
    }

    [TestMethod]
    public async Task StoresAllowedFieldsGivenValidNewUser()
    {
        var client = HttpClientHelper.GetAdminClient();
        var lockoutEnd = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var newUser = new UserDto
        {
            UserName = $"{Guid.NewGuid()}@example.com",
            Email = $"{Guid.NewGuid()}@example.com",
            PhoneNumber = "555-0100",
            LockoutEnd = lockoutEnd
        };

        var userId = await CreateUserAsync(client, newUser);

        var stored = await GetUserByIdAsync(client, userId);
        Assert.AreEqual(newUser.UserName, stored.UserName);
        Assert.AreEqual(newUser.Email, stored.Email);
        Assert.AreEqual(newUser.PhoneNumber, stored.PhoneNumber);
        Assert.AreEqual(lockoutEnd, stored.LockoutEnd);
    }

    private static async Task<string> CreateUserAsync(HttpClient client, UserDto user)
    {
        var response = await client.PostAsync("api/users", JsonContent.Create(new CreateUserRequest { User = user }));
        Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        return (await response.Content.ReadAsStringAsync()).FromJson<CreateUserResponse>()!.UserId;
    }

    private static async Task<UserDto> GetUserByIdAsync(HttpClient client, string userId)
    {
        var response = await client.GetAsync($"api/users/{userId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<GetUserResponse>()!.User;
    }

    private StringContent GetValidNewItemJson(string userName)
    {
        var newUser = new UserDto()
        {
            UserName = userName
        };
        var request = new CreateUserRequest
        {
            User = newUser
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        return jsonContent;
    }

    private StringContent GetInvalidNewItemJson()
    {
        var newUser = new UserDto()
        {

        };
        var request = new CreateUserRequest
        {
            User = newUser
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

        return jsonContent;
    }
}
