namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class DeleteUserRequest : BaseRequest
{
    public string UserId { get; init; }

    public DeleteUserRequest(string userId)
    {
        UserId = userId;
    }
}
