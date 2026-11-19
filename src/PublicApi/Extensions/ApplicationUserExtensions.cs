using System;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.PublicApi.UserManagementEndpoints;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

public static class ApplicationUserExtensions
{
    // Case-insensitive, like Identity user name lookups, so a case variant cannot slip past.
    public static bool IsDefaultAdmin(this ApplicationUser user) =>
        string.Equals(user.UserName, AuthorizationConstants.DEFAULT_ADMIN_USERNAME, StringComparison.OrdinalIgnoreCase);

    public static UserDto ToUserDto(this ApplicationUser user)
    {
        UserDto dto = new UserDto();
        if (user != null)
        {
            dto.Id = user.Id;
            dto.UserName = user.UserName;
            dto.Email = user.Email;
            dto.PhoneNumber = user.PhoneNumber;
            dto.EmailConfirmed = user.EmailConfirmed;
            dto.PhoneNumberConfirmed = user.PhoneNumberConfirmed;
            dto.TwoFactorEnabled = user.TwoFactorEnabled;
            dto.LockoutEnd = user.LockoutEnd;

        }
        return dto;
    }
}
