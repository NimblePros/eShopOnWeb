using System;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class CreateUserResponse : BaseResponse
{
    public CreateUserResponse(Guid correlationId)
    {

    }
    public CreateUserResponse() { }

    public UserDto User { get; set; }
}
