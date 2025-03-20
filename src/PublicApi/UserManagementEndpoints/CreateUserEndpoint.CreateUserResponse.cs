using System;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class CreateUserResponse : BaseResponse
{
    public CreateUserResponse(Guid correlationId)
    {

    }
    public CreateUserResponse() { }

    public ApplicationUser User { get; set; }
}
