using System;

namespace Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

public class UserDto()
{
   public string Id {  get; set; }
    public string? UserName { get; set; }
    public string? Email { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }    
}
