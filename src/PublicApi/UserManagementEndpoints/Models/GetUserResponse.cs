using System;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;

public class GetUserResponse : BaseResponse
{

    public GetUserResponse(Guid correlationId) : base(correlationId)
    {
    }

    public GetUserResponse()
    {
    }

    public UserDto User { get; set; }
}
