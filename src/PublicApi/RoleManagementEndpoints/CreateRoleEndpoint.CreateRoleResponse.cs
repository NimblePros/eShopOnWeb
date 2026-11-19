using System;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class CreateRoleResponse : BaseResponse
{
    public CreateRoleResponse(Guid correlationId)
    {

    }
    public CreateRoleResponse() { }

    public RoleDto Role { get; set; }
}
