using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.RoleManagementEndpoints;

[TestClass]
public class UpdateRoleEndpointTest
{
    private const string Route = "api/roles";

    [TestMethod]
    public async Task ReturnsUnauthorizedGivenNoToken()
    {
        var client = ProgramTest.NewClient;

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenNonExistentIdAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(Guid.NewGuid().ToString())));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task RenamesRoleGivenValidIdAndAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var roleId = await CreateRoleAsync(client);
        var request = BuildRequest(roleId);

        var response = await client.PutAsync(Route, JsonContent.Create(request));

        response.EnsureSuccessStatusCode();
        var updated = (await response.Content.ReadAsStringAsync()).FromJson<UpdateRoleResponse>()!.Role;
        Assert.AreEqual(roleId, updated.Id);
        Assert.AreEqual(request.Name, updated.Name);

        var getResponse = await client.GetAsync($"{Route}/{roleId}");
        getResponse.EnsureSuccessStatusCode();
        var persisted = (await getResponse.Content.ReadAsStringAsync()).FromJson<GetByIdRoleResponse>()!.Role;
        Assert.AreEqual(request.Name, persisted.Name);
    }

    private static UpdateRoleRequest BuildRequest(string id) => new()
    {
        Id = id,
        Name = $"renamed role {Guid.NewGuid()}"
    };

    // Creates a dedicated role so renaming it doesn't affect the seeded roles other tests rely on.
    private static async Task<string> CreateRoleAsync(HttpClient client)
    {
        var createRequest = new CreateRoleRequest { Name = $"role to rename {Guid.NewGuid()}" };
        var response = await client.PostAsync(Route, JsonContent.Create(createRequest));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<CreateRoleResponse>()!.Role.Id;
    }
}
