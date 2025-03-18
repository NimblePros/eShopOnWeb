namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class DeleteUserFromRoleRequest : BaseRequest
{
    public string UserId { get; init; }
    public string RoleName { get; init; }

    public DeleteUserFromRoleRequest(string userId, string roleName)
    {
        UserId = userId;
        RoleName = roleName;
    }
}
