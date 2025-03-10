using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class RoleListEndpointResponse : BaseResponse
{
    public RoleListEndpointResponse(Guid correlationId) : base(correlationId)
    {
    }

    public RoleListEndpointResponse()
    {
    }

    public List<IdentityRole> Roles { get; set; } = new List<IdentityRole>();
}
