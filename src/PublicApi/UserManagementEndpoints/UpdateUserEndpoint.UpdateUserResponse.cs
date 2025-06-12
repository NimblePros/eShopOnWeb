using System;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UpdateUserResponse : BaseResponse
{
    public UpdateUserResponse(Guid correlationId) : base(correlationId)
    {
    }

    public UpdateUserResponse()
    {
    }
    public ApplicationUser User { get; set; }
}
