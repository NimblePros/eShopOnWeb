using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Extensions;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

[Authorize]
public class ExternalLoginsModel(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAppLogger<ExternalLoginsModel> logger) : PageModel
{
    public IList<UserLoginInfo> CurrentLogins { get; private set; } = [];

    public IList<AuthenticationScheme> OtherLogins { get; private set; } = [];

    public bool ShowRemoveButton { get; private set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        CurrentLogins = await userManager.GetLoginsAsync(user);
        OtherLogins = (await signInManager.GetExternalAuthenticationSchemesAsync())
            .Where(auth => CurrentLogins.All(ul => auth.Name != ul.LoginProvider))
            .ToList();
        ShowRemoveButton = await userManager.HasPasswordAsync(user) || CurrentLogins.Count > 1;
    }

    public async Task<IActionResult> OnPostLinkLoginAsync(string provider)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Request a redirect to the external login provider to link a login for the current user
        var user = await userManager.GetRequiredUserAsync(User);

        // Only log (and challenge) providers the app actually offers, so a posted value can't forge a log entry.
        var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();
        if (!schemes.Any(scheme => scheme.Name == provider))
        {
            logger.LogWarning("User with ID {UserId} requested to link an unknown external login provider.", user.Id);
            StatusMessage = "Error: The external login provider is not available.";
            return RedirectToPage();
        }

        logger.LogInformation("User with ID {UserId} requested to link an external login from {LoginProvider}.", user.Id, provider);
        var redirectUrl = Url.Page("./ExternalLogins", pageHandler: "LinkLoginCallback");
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl, user.Id);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetLinkLoginCallbackAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        var info = await signInManager.GetExternalLoginInfoAsync(user.Id);
        if (info == null)
        {
            logger.LogWarning("Linking an external login failed for user with ID {UserId}: no external login info was returned.", user.Id);
            throw new ApplicationException($"Unexpected error occurred loading external login info for user with ID '{user.Id}'.");
        }

        var result = await userManager.AddLoginAsync(user, info);
        if (!result.Succeeded)
        {
            logger.LogWarning("Linking an external login from {LoginProvider} failed for user with ID {UserId}.", info.LoginProvider, user.Id);
            throw new ApplicationException($"Unexpected error occurred adding external login for user with ID '{user.Id}'.");
        }

        if (info.Principal.Claims.Any())
        {
            var claimsToSync = new Dictionary<string, string>();
            foreach (var claim in info.Principal.Claims)
            {
                claimsToSync.Add(claim.Type, claim.Value);
            }
            var refreshSignIn = await userManager.SaveClaimsAsync(claimsToSync, info, user);
            if (refreshSignIn)
            {
                await signInManager.RefreshSignInAsync(user);
            }
        }

        logger.LogInformation("User with ID {UserId} linked an external login from {LoginProvider}.", user.Id, info.LoginProvider);

        // Clear the external cookie now that the login is linked
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        StatusMessage = "The external login was added.";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveLoginAsync(string loginProvider, string providerKey)
    {
        var user = await userManager.GetRequiredUserAsync(User);

        if (string.IsNullOrEmpty(loginProvider) || string.IsNullOrEmpty(providerKey))
        {
            StatusMessage = "Error: The external login to remove was not specified.";
            return RedirectToPage();
        }

        // Look the login up so only a provider the user really has is logged or removed.
        var logins = await userManager.GetLoginsAsync(user);
        var login = logins.FirstOrDefault(l => l.LoginProvider == loginProvider && l.ProviderKey == providerKey);
        if (login == null)
        {
            logger.LogWarning("User with ID {UserId} tried to remove an external login they don't have.", user.Id);
            StatusMessage = "Error: The external login to remove was not found.";
            return RedirectToPage();
        }

        // The page only offers Remove when the user can still sign in afterwards; enforce that here too.
        var canRemove = await userManager.HasPasswordAsync(user) || logins.Count > 1;
        if (!canRemove)
        {
            logger.LogWarning("Refused to remove the only sign-in method ({LoginProvider}) of user with ID {UserId}.", login.LoginProvider, user.Id);
            StatusMessage = "Error: You can't remove your only way to sign in. Set a password first.";
            return RedirectToPage();
        }

        var result = await userManager.RemoveLoginAsync(user, login.LoginProvider, login.ProviderKey);
        if (!result.Succeeded)
        {
            logger.LogWarning("Removing the external login from {LoginProvider} failed for user with ID {UserId}.", login.LoginProvider, user.Id);
            throw new ApplicationException($"Unexpected error occurred removing external login for user with ID '{user.Id}'.");
        }

        logger.LogInformation("User with ID {UserId} removed an external login from {LoginProvider}.", user.Id, login.LoginProvider);
        await signInManager.SignInAsync(user, isPersistent: false);
        StatusMessage = "The external login was removed.";
        return RedirectToPage();
    }
}
