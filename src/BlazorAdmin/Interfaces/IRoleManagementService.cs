using System.Collections.Generic;
using System.Threading.Tasks;
using BlazorAdmin.Models;
using Microsoft.AspNetCore.Identity;

namespace BlazorAdmin.Interfaces;

public interface IRoleManagementService
{
    Task<CreateRoleResponse> Create(CreateRoleRequest role);
    Task<IdentityRole> Edit(IdentityRole role);
    Task<string> Delete(string id);
    Task<IdentityRole> GetById(string id);
    Task<RoleListResponse> List();
}
