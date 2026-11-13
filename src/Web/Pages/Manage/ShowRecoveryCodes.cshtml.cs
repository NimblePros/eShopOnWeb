using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

/// <summary>
/// Shows recovery codes that another page just generated and stored in TempData. They are shown
/// once; revisiting the page sends the user back to the 2FA overview.
/// </summary>
[Authorize]
public class ShowRecoveryCodesModel : PageModel
{
    public const string RecoveryCodesKey = nameof(RecoveryCodesKey);

    public string[] RecoveryCodes { get; private set; } = [];

    public IActionResult OnGet()
    {
        if (TempData[RecoveryCodesKey] is not string[] recoveryCodes || recoveryCodes.Length == 0)
        {
            return RedirectToPage("./TwoFactorAuthentication");
        }

        RecoveryCodes = recoveryCodes;
        return Page();
    }
}
