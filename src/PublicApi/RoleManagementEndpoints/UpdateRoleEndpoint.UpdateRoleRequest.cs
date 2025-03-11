namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class UpdateRoleRequest : BaseRequest
{
    public string Name { get; set; }
    public string Id { get; set; }
}
