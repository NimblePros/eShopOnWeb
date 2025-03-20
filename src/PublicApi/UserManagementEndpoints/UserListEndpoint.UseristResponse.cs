using System;
using System.Collections.Generic;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UserListResponse : BaseResponse
{
    public UserListResponse(Guid correlationId) : base(correlationId)
    {
    }

    public UserListResponse()
    {
    }

    public List<ApplicationUser> Users{ get; set; } = [];
}
