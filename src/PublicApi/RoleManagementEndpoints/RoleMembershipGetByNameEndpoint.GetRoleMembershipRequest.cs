namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

public class GetRoleMembershipRequest : BaseRequest
{
    public string RoleName { get; init; }

    public GetRoleMembershipRequest(string name)
    {
        RoleName = name;
    }
}
