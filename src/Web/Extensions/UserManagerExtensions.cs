using System.Security.Claims;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.Web.Extensions;

public static class UserManagerExtensions
{
    public static async Task<bool> SaveClaimsAsync(this UserManager<ApplicationUser> userManager, Dictionary<string, string> claimsToSync, ExternalLoginInfo info, ApplicationUser user)
    {
        Guard.Against.Null(user);
        Guard.Against.Null(info);
        Guard.Against.Null(info.Principal);

        var refreshSignIn = false;
        var userClaims = await userManager.GetClaimsAsync(user);

        foreach (var addedClaim in claimsToSync)
        {
            var userClaim = userClaims
                .FirstOrDefault(c => c.Type == addedClaim.Key);

            var externalClaim = info.Principal.FindFirst(addedClaim.Key);
            Guard.Against.Null(externalClaim);
            if (userClaim is null)
            {
                await userManager.AddClaimAsync(user,
                    new Claim(addedClaim.Key, externalClaim.Value));
                refreshSignIn = true;
            }
            else if (userClaim.Value != externalClaim.Value)
            {
                await userManager
                    .ReplaceClaimAsync(user, userClaim, externalClaim);
                refreshSignIn = true;
            }

        }
        return refreshSignIn;
    }
}
