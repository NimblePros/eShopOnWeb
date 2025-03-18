using System.Linq;
using System.Net;
using System.Net.Http;
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
    public async Task ReturnsNotFoundGivenValidRoleIdAndInvalidUserIdAndAdminUserToken()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);
        var validRoleId = await GetValidRoleId(client, Constants.Roles.ADMINISTRATORS);

        var response = await client.DeleteAsync($"api/roles/{validRoleId}/members/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidRoleIdAndValidUserIdAndAdminUserToken()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var roleList = await client.GetAsync($"/api/roles/{Constants.Roles.ADMINISTRATORS}/members");
        var roleMembersResponse = await roleList.Content.ReadAsStringAsync();
        var validUsers = roleMembersResponse.FromJson<GetRoleMembershipResponse>();

        var validUserId = validUsers!.RoleMembers.FirstOrDefault()!.Id;

        var response = await client.DeleteAsync($"api/roles/0/members/{validUserId}");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsOkWhenDeletingUserFromRoleSuccessfully()
    {
        var adminToken = ApiTokenHelper.GetAdminUserToken();
        var client = ProgramTest.NewClient;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var roleName = Constants.Roles.PRODUCT_MANAGERS;
        var validRoleId = await GetValidRoleId(client, Constants.Roles.PRODUCT_MANAGERS);

        var roleList = await client.GetAsync($"/api/roles/{roleName}/members");
        var roleMembersResponse = await roleList.Content.ReadAsStringAsync();
        var validUsers = roleMembersResponse.FromJson<GetRoleMembershipResponse>();

        var validUserId = validUsers!.RoleMembers.FirstOrDefault()!.Id;

        var response = await client.DeleteAsync($"api/roles/{validRoleId}/members/{validUserId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<string> GetValidRoleId(HttpClient client, string roleName)
    {
        var validRoles = await client.GetAsync("/api/roles");
        validRoles.EnsureSuccessStatusCode();
        var validRolesCollection = await validRoles.Content.ReadAsStringAsync();
        var model = validRolesCollection.FromJson<RoleListResponse>();
        var selectedRole = model!.Roles.Where(x => x.Name == roleName).First();
        return selectedRole.Id;
    }
}
