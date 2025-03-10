using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class RoleListResponse : BaseResponse
{
    public RoleListResponse(Guid correlationId) : base(correlationId)
    {
    }

    public RoleListResponse()
    {
    }

    public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
}
