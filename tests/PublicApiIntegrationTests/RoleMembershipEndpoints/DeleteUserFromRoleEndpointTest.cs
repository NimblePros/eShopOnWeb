using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using BlazorShared.Authorization;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.RoleMembershipEndpoints;

[TestClass]
public class DeleteUserFromRoleEndpointTest
{
    [TestMethod]
    public async Task ReturnsNotFoundGivenValidRoleNameAndInvalidIdAndAdminUserToken()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var validRoleName = Constants.Roles.ADMINISTRATORS;
        var response = await client.DeleteAsync($"api/roles/{validRoleName}/members/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidRoleNameAndValidIdAndAdminUserToken()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var roleList = await client.GetAsync($"/api/roles/{Constants.Roles.ADMINISTRATORS}/members");
        var roleMembersResponse = await roleList.Content.ReadAsStringAsync();
        var validUsers = roleMembersResponse.FromJson<GetRoleMembershipResponse>();

        var validId = validUsers!.RoleMembers.FirstOrDefault()!.Id;

        var response = await client.DeleteAsync($"api/roles/invalidRoleName/members/{validId}");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsOkWhenDeletingUserFromRoleSuccessfully()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var roleName = Constants.Roles.PRODUCT_MANAGERS;

        var roleList = await client.GetAsync($"/api/roles/{roleName}/members");
        var roleMembersResponse = await roleList.Content.ReadAsStringAsync();
        var validUsers = roleMembersResponse.FromJson<GetRoleMembershipResponse>();

        var validId = validUsers!.RoleMembers.FirstOrDefault()!.Id;

        var response = await client.DeleteAsync($"api/roles/{roleName}/members/{validId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
