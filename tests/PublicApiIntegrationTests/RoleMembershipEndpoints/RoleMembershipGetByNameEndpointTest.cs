using System.Net.Http.Headers;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using BlazorShared.Authorization;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.RoleMembershipEndpoints;
[TestClass]
public class RoleMembershipGetByNameEndpointTest : PublicApiTestBase
{
    [TestMethod]
    public async Task ReturnsMembersWithValidRoleName()
    {
        var client = GetAdminClient();

        var roleList = await client.GetAsync($"/api/roles/{Constants.Roles.ADMINISTRATORS}/members");
        var roleMembersResponse = await roleList.Content.ReadAsStringAsync();
        var model = roleMembersResponse.FromJson<GetRoleMembershipResponse>();
        Assert.IsNotNull(model);
        Assert.IsTrue(model.RoleMembers.Count > 0);
    }

    [TestMethod]
    public async Task ReturnsEmptyListGivenInvalidName()
    {
        var client = GetAdminClient();

        var invalidName = "invalidName";
        var getInvalidRoleNameMembership = await client.GetAsync($"api/roles/{invalidName}/members");
        var response = await getInvalidRoleNameMembership.Content.ReadAsStringAsync();
        var model = response.FromJson<GetRoleMembershipResponse>();
        Assert.IsNotNull(model);
        Assert.AreEqual(0, model.RoleMembers.Count);
    }
}
