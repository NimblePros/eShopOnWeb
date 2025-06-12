namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UpdateUserRequest : BaseRequest
{
    public UserDto User { get; set; }
}
