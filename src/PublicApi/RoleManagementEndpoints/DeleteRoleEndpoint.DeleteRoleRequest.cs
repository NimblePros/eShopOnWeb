using System;

namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class DeleteRoleRequest : BaseRequest
{
    public string RoleId { get; init; }

    public DeleteRoleRequest(string roleId)
    {
        RoleId = roleId;
    }
}
