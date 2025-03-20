using System;
using System.Collections.Generic;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UserListResponse : BaseResponse
{
    public UserListResponse(Guid correlationId) : base(correlationId)
    {
    }

    public UserListResponse()
    {
    }

    public List<UserDto> Users{ get; set; } = [];
}
