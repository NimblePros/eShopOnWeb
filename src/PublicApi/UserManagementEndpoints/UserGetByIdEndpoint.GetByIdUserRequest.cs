namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class GetByIdUserRequest : BaseRequest
{
    public string UserId { get; init; }

    public GetByIdUserRequest(string userId)
    {
        UserId = userId;
    }
}
