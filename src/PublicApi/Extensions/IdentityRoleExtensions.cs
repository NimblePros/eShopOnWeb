using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

public static class IdentityRoleExtensions
{
    public static RoleDto ToRoleDto(this IdentityRole role) => new() { Id = role.Id, Name = role.Name };
}
