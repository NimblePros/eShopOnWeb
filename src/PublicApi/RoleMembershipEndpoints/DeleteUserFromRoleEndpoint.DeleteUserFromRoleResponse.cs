using System;

namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class DeleteUserFromRoleResponse : BaseResponse
{
    public DeleteUserFromRoleResponse(Guid correlationId) : base(correlationId)
    {
    }

    public DeleteUserFromRoleResponse()
    {
    }

    public string Status { get; set; } = "Deleted";
}
