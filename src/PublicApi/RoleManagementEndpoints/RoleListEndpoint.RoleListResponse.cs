using System;
using System.Collections.Generic;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class RoleListResponse : BaseResponse
{
    public RoleListResponse(Guid correlationId) : base(correlationId)
    {
    }

    public RoleListResponse()
    {
    }

    public List<RoleDto> Roles { get; set; } = [];
}
