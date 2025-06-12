namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class GetRoleMembershipRequest : BaseRequest
{
    public string RoleName { get; init; }

    public GetRoleMembershipRequest(string name)
    {
        RoleName = name;
    }
}
