using System.Threading.Tasks;
using BlazorAdmin.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Interfaces;

public interface IRoleManagementService
{
    Task<CreateRoleResponse> Create(CreateRoleRequest role);
    Task<IdentityRole> Edit(IdentityRole role);
    Task Delete(string id);
    Task DeleteUserFromRole(string userId, string roleId);
    Task<GetByIdRoleResponse> GetById(string id);
    Task<GetRoleMembershipResponse> GetMembershipByName(string name);
    Task<RoleListResponse> List();
}
