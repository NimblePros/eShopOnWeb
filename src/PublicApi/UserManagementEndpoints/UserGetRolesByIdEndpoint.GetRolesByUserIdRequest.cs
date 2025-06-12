namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class GetRolesByUserIdRequest : BaseRequest
{
    public string UserId { get; init; }

    public GetRolesByUserIdRequest(string userId)
    {
        UserId = userId;
    }
}
