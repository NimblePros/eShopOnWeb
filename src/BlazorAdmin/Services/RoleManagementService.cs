using System.Threading.Tasks;
using BlazorAdmin.Interfaces;
using BlazorAdmin.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace BlazorAdmin.Services;

public class RoleManagementService(HttpService httpService, ILogger<RoleManagementService> logger) : IRoleManagementService
{
    public async Task<RoleListResponse> List(){
        logger.LogInformation("Fetching roles");
        var response = await httpService.HttpGet<RoleListResponse>($"roles");
        return response;
    }

    public async Task<CreateRoleResponse> Create(CreateRoleRequest newRole)
    {
        var response = await httpService.HttpPost<CreateRoleResponse>($"roles", newRole);
        return response;
    }

    public async Task<IdentityRole> Edit(IdentityRole role)
    {
        return await httpService.HttpPut<IdentityRole>($"roles", role);
    }

    public async Task<string> Delete(string id)
    {
        var response = await httpService.HttpDelete<DeleteRoleResponse>($"roles", id);
        return response.Status;
    }

    public async Task<GetByIdRoleResponse> GetById(string id)
    {
        var roleById = await httpService.HttpGet<GetByIdRoleResponse>($"roles/{id}");
        return roleById;
    }

    public async Task<GetRoleMembershipResponse> GetMembershipByName(string name)
    {
        var roleMembershipByName = await httpService.HttpGet<GetRoleMembershipResponse>($"roles/{name}/members");
        return roleMembershipByName;
    }

    public async Task<string> DeleteUserFromRole(string userId, string roleId)
    {
        var response = await httpService.HttpDelete<DeleteUserFromRoleResponse>($"roles/{roleId}/members/{userId}");
        return response.Status;
    }
}
