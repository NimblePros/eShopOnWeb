namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class GetByIdRoleRequest : BaseRequest
{
    public string RoleId { get; init; }

    public GetByIdRoleRequest(string roleId)
    {
        RoleId = roleId;
    }
}
