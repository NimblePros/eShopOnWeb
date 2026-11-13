using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Extensions;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

[Authorize]
public class ChangePasswordModel(UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    IAppLogger<ChangePasswordModel> logger) : PageModel
{
    [BindProperty]
    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Current password")]
    public string? OldPassword { get; set; }

    [BindProperty]
    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "New password")]
    public string? NewPassword { get; set; }

    [BindProperty]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm new password")]
    [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
    public string? ConfirmPassword { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);

        if (!await userManager.HasPasswordAsync(user))
        {
            return RedirectToPage("./SetPassword");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var user = await userManager.GetRequiredUserAsync(User);

        var changePasswordResult = await userManager.ChangePasswordAsync(user, OldPassword!, NewPassword!);
        if (!changePasswordResult.Succeeded)
        {
            ModelState.AddIdentityErrors(changePasswordResult);
            return Page();
        }

        await signInManager.SignInAsync(user, isPersistent: false);
        logger.LogInformation("User changed their password successfully.");
        StatusMessage = "Your password has been changed.";

        return RedirectToPage();
    }
}
