using System;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class DeleteRoleResponse : BaseResponse
{
    public DeleteRoleResponse(Guid correlationId) : base(correlationId)
    {
    }

    public DeleteRoleResponse()
    {
    }

    public string Status { get; set; } = "Deleted";
}
