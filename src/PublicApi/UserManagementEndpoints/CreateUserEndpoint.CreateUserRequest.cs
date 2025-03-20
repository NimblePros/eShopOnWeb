using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class CreateUserRequest : BaseRequest
{
    public ApplicationUser User { get; set; }
}
