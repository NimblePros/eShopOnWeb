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

    public async Task Delete(string id)
    {
        await httpService.HttpDelete($"roles/{id}");        
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

    public async Task DeleteUserFromRole(string userId, string roleId)
    {
        await httpService.HttpDelete($"roles/{roleId}/members/{userId}");        
    }
}
