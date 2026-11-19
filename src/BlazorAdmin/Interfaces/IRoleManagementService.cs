using System.Threading.Tasks;
using BlazorAdmin.Models;

namespace BlazorAdmin.Interfaces;

public interface IRoleManagementService
{
    Task<CreateRoleResponse> Create(CreateRoleRequest role);
    Task<UpdateRoleResponse> Edit(Role role);
    Task Delete(string id);
    Task DeleteUserFromRole(string userId, string roleId);
    Task<GetByIdRoleResponse> GetById(string id);
    Task<GetRoleMembershipResponse> GetMembershipByName(string name);
    Task<RoleListResponse> List();
}
