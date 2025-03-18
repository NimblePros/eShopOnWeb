namespace Microsoft.eShopWeb.PublicApi.RoleMembershipEndpoints;

public class DeleteUserFromRoleRequest : BaseRequest
{
    public string UserId { get; init; }
    public string RoleId { get; init; }

    public DeleteUserFromRoleRequest(string userId, string roleId)
    {
        UserId = userId;
        RoleId = roleId;
    }
}
