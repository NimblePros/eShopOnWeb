using System;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class GetByIdRoleResponse : BaseResponse
{

    public GetByIdRoleResponse(Guid correlationId) : base(correlationId)
    {
    }

    public GetByIdRoleResponse()
    {
    }

    public RoleDto Role { get; set; }
}
