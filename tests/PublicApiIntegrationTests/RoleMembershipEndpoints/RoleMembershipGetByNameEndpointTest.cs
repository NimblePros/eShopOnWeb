using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Authorization;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.RoleMembershipEndpoints;

[TestClass]
public class RoleMembershipGetByNameEndpointTest
{
    [TestMethod]
    public async Task ReturnsMembersWithValidRoleName()
    {
        var client = HttpClientHelper.GetAdminClient();

        var roleList = await client.GetAsync($"/api/roles/{Constants.Roles.ADMINISTRATORS}/members");
        var roleMembersResponse = await roleList.Content.ReadAsStringAsync();
        var model = roleMembersResponse.FromJson<GetRoleMembershipResponse>();
        Assert.IsNotNull(model);
        Assert.IsTrue(model.RoleMembers.Count > 0);
    }

    [TestMethod]
    public async Task DoesNotExposeSensitiveIdentityFieldsForMembers()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.GetAsync($"/api/roles/{Constants.Roles.ADMINISTRATORS}/members");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.IsTrue(json.RootElement.TryGetProperty("roleMembers", out var members));
        Assert.IsTrue(members.GetArrayLength() > 0);
        foreach (var member in members.EnumerateArray())
        {
            UserJsonAssert.HasOnlyUserDtoFields(member);
        }
    }

    [TestMethod]
    public async Task ReturnsEmptyListGivenInvalidName()
    {
        var client = HttpClientHelper.GetAdminClient();

        var invalidName = "invalidName";
        var getInvalidRoleNameMembership = await client.GetAsync($"api/roles/{invalidName}/members");
        var response = await getInvalidRoleNameMembership.Content.ReadAsStringAsync();
        var model = response.FromJson<GetRoleMembershipResponse>();
        Assert.IsNotNull(model);
        Assert.AreEqual(0, model.RoleMembers.Count);
    }
}
