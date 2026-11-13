using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Extensions;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

/// <summary>
/// GET shows the warning; POST disables two-factor authentication.
/// </summary>
[Authorize]
public class Disable2faModel(UserManager<ApplicationUser> userManager,
    IAppLogger<Disable2faModel> logger) : PageModel
{
    public async Task OnGetAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        if (!user.TwoFactorEnabled)
        {
            throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        var disable2faResult = await userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            throw new ApplicationException($"Unexpected error occured disabling 2FA for user with ID '{user.Id}'.");
        }

        logger.LogInformation("User with ID {UserId} has disabled 2fa.", user.Id);
        return RedirectToPage("./TwoFactorAuthentication");
    }
}
