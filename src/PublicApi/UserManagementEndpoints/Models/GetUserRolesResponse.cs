using System;
using System.Collections.Generic;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;

public class GetUserRolesResponse : BaseResponse
{

    public GetUserRolesResponse(Guid correlationId) : base(correlationId)
    {
    }

    public GetUserRolesResponse()
    {
    }

    public List<string> Roles {  get; set; }
}
