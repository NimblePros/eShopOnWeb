using System;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class UpdateRoleResponse : BaseResponse
{
    public UpdateRoleResponse(Guid correlationId) : base(correlationId)
    {
    }

    public UpdateRoleResponse()
    {
    }
    public RoleDto Role { get; set; }
}
