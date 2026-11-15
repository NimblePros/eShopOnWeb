using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;
using Microsoft.Extensions.DependencyInjection;
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
    public async Task ReturnsBadRequestGivenDefaultAdminUserAndLeavesItUnchanged()
    {
        var client = HttpClientHelper.GetAdminClient();
        var admin = await GetUserByNameAsync(client, AuthorizationConstants.DEFAULT_ADMIN_USERNAME);
        // Only LockoutEnd changes (to a time already past), so a regression here can't rename
        // or lock out the seeded admin that other tests sign in as.
        var lockoutEnd = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        admin.LockoutEnd = lockoutEnd;

        var response = await client.PutAsync("api/users", JsonContent.Create(new UpdateUserRequest { User = admin }));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var adminAfter = await GetUserByNameAsync(client, AuthorizationConstants.DEFAULT_ADMIN_USERNAME);
        Assert.AreNotEqual(lockoutEnd, adminAfter.LockoutEnd);
    }

    [TestMethod]
    public async Task IgnoresConfirmationFlagsInRequest()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        // Send the current values so only the confirmation flags differ.
        var user = await GetUserByIdAsync(client, userId);
        user.EmailConfirmed = true;
        user.PhoneNumberConfirmed = true;

        var response = await client.PutAsync("api/users", JsonContent.Create(new UpdateUserRequest { User = user }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var stored = await GetUserByIdAsync(client, userId);
        Assert.IsFalse(stored.EmailConfirmed);
        Assert.IsFalse(stored.PhoneNumberConfirmed);
    }

    [TestMethod]
    public async Task KeepsEmailConfirmedGivenUnchangedEmail()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateConfirmedUserAsync(client);
        var user = await GetUserByIdAsync(client, userId);
        user.PhoneNumber = "555-0100";

        var response = await client.PutAsync("api/users", JsonContent.Create(new UpdateUserRequest { User = user }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsTrue((await GetUserByIdAsync(client, userId)).EmailConfirmed);
    }

    [TestMethod]
    public async Task ResetsEmailConfirmedGivenChangedEmail()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateConfirmedUserAsync(client);
        var user = await GetUserByIdAsync(client, userId);
        user.Email = $"{Guid.NewGuid()}@example.com";

        var response = await client.PutAsync("api/users", JsonContent.Create(new UpdateUserRequest { User = user }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.IsFalse((await GetUserByIdAsync(client, userId)).EmailConfirmed);
    }

    [TestMethod]
    public async Task RotatesSecurityStampGivenUserNameChange()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        var stampBefore = await GetSecurityStampAsync(userId);

        var response = await client.PutAsync("api/users", JsonContent.Create(UpdateRequest(userId)));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreNotEqual(stampBefore, await GetSecurityStampAsync(userId));
    }

    [TestMethod]
    public async Task RotatesSecurityStampGivenLockoutChangeOnly()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        var user = await GetUserByIdAsync(client, userId);
        user.LockoutEnd = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var stampBefore = await GetSecurityStampAsync(userId);

        var response = await client.PutAsync("api/users", JsonContent.Create(new UpdateUserRequest { User = user }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreNotEqual(stampBefore, await GetSecurityStampAsync(userId));
    }

    [TestMethod]
    public async Task KeepsSecurityStampGivenNoChangesAndEmptyPhoneNumber()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        var user = await GetUserByIdAsync(client, userId);
        user.PhoneNumber = ""; // what an untouched form field posts
        var stampBefore = await GetSecurityStampAsync(userId);

        var response = await client.PutAsync("api/users", JsonContent.Create(new UpdateUserRequest { User = user }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(stampBefore, await GetSecurityStampAsync(userId));
    }

    [TestMethod]
    public async Task ReturnsBadRequestGivenDuplicateUserName()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        var otherUser = await GetUserByIdAsync(client, await CreateUserAsync(client));
        var before = await GetUserByIdAsync(client, userId);
        var request = UpdateRequest(userId);
        request.User.UserName = otherUser.UserName;

        var response = await client.PutAsync("api/users", JsonContent.Create(request));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var after = await GetUserByIdAsync(client, userId);
        Assert.AreEqual(before.UserName, after.UserName);
        Assert.AreEqual(before.Email, after.Email);
    }

    [TestMethod]
    public async Task UpdatesLockoutEndGivenNormalUser()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        var current = await GetUserByIdAsync(client, userId);
        var lockoutEnd = new DateTimeOffset(2099, 1, 1, 0, 0, 0, TimeSpan.Zero);
        current.LockoutEnd = lockoutEnd;

        var response = await client.PutAsync("api/users", JsonContent.Create(new UpdateUserRequest { User = current }));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.AreEqual(lockoutEnd, (await GetUserByIdAsync(client, userId)).LockoutEnd);
    }

    [TestMethod]
    public async Task PersistsChangesGivenValidChangesAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var userId = await CreateUserAsync(client);
        var request = UpdateRequest(userId);

        var response = await client.PutAsync("api/users", JsonContent.Create(request));

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        var stored = await GetUserByIdAsync(client, userId);
        Assert.AreEqual(request.User.UserName, stored.UserName);
        Assert.AreEqual(request.User.Email, stored.Email);
        Assert.AreEqual(request.User.PhoneNumber, stored.PhoneNumber);
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

    private static async Task<UserDto> GetUserByIdAsync(HttpClient client, string userId)
    {
        var response = await client.GetAsync($"api/users/{userId}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<GetUserResponse>()!.User;
    }

    private static async Task<UserDto> GetUserByNameAsync(HttpClient client, string userName)
    {
        var response = await client.GetAsync($"api/users/name/{userName}");
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<GetUserResponse>()!.User;
    }

    private static async Task<string> CreateConfirmedUserAsync(HttpClient client)
    {
        var userId = await CreateUserAsync(client);
        using var scope = ProgramTest.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        user!.Email = $"{Guid.NewGuid()}@example.com";
        user.EmailConfirmed = true;
        var result = await userManager.UpdateAsync(user);
        Assert.IsTrue(result.Succeeded);
        return userId;
    }

    // The security stamp is not part of the API, so read it straight from Identity.
    private static async Task<string?> GetSecurityStampAsync(string userId)
    {
        using var scope = ProgramTest.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId);
        return user!.SecurityStamp;
    }
}
