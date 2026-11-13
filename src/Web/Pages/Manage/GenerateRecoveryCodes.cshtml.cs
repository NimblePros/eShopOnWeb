using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Extensions;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

/// <summary>
/// GET shows the warning; POST generates a new set of recovery codes and shows them once.
/// </summary>
[Authorize]
public class GenerateRecoveryCodesModel(UserManager<ApplicationUser> userManager,
    IAppLogger<GenerateRecoveryCodesModel> logger) : PageModel
{
    public async Task OnGetAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        if (!user.TwoFactorEnabled)
        {
            throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' because they do not have 2FA enabled.");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        if (!user.TwoFactorEnabled)
        {
            throw new ApplicationException($"Cannot generate recovery codes for user with ID '{user.Id}' as they do not have 2FA enabled.");
        }

        var recoveryCodes = await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10) ?? [];
        logger.LogInformation("User with ID {UserId} has generated new 2FA recovery codes.", user.Id);

        TempData[ShowRecoveryCodesModel.RecoveryCodesKey] = recoveryCodes.ToArray();
        return RedirectToPage("./ShowRecoveryCodes");
    }
}
