using System;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UpdateUserResponse : BaseResponse
{
    public UpdateUserResponse(Guid correlationId) : base(correlationId)
    {
    }

    public UpdateUserResponse()
    {
    }
    public UserDto User { get; set; }
}
