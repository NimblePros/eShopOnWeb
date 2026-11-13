using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Extensions;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

[Authorize]
public class TwoFactorAuthenticationModel(UserManager<ApplicationUser> userManager) : PageModel
{
    public bool HasAuthenticator { get; private set; }

    public int RecoveryCodesLeft { get; private set; }

    public bool Is2faEnabled { get; private set; }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        HasAuthenticator = await userManager.GetAuthenticatorKeyAsync(user) != null;
        Is2faEnabled = user.TwoFactorEnabled;
        RecoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
    }
}
