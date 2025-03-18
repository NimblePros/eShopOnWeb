using System;
using System.Collections.Generic;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class GetRoleMembershipResponse : BaseResponse
{

    public GetRoleMembershipResponse(Guid correlationId) : base(correlationId)
    {
    }

    public GetRoleMembershipResponse()
    {
    }

    public List<ApplicationUser> RoleMembers { get; set; }
}
