using System;
using System.Collections.Generic;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class GetRoleMembershipResponse : BaseResponse
{

    public GetRoleMembershipResponse(Guid correlationId) : base(correlationId)
    {
    }

    public GetRoleMembershipResponse()
    {
    }

    public List<UserDto> RoleMembers { get; set; }
}
