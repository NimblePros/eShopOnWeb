using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Extensions;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

/// <summary>
/// GET shows the warning; POST disables 2FA and resets the authenticator key.
/// </summary>
[Authorize]
public class ResetAuthenticatorModel(UserManager<ApplicationUser> userManager,
    IAppLogger<ResetAuthenticatorModel> logger) : PageModel
{
    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        await userManager.SetTwoFactorEnabledAsync(user, false);
        await userManager.ResetAuthenticatorKeyAsync(user);
        logger.LogInformation("User with id '{UserId}' has reset their authentication app key.", user.Id);

        return RedirectToPage("./EnableAuthenticator");
    }
}
