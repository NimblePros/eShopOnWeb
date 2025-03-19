using System;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints.Models;

public class GetUserResponse : BaseResponse
{

    public GetUserResponse(Guid correlationId) : base(correlationId)
    {
    }

    public GetUserResponse()
    {
    }

    public ApplicationUser User { get; set; }
}
