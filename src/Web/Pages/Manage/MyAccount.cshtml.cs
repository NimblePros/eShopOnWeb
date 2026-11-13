using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Extensions;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

[Authorize]
public class MyAccountModel(UserManager<ApplicationUser> userManager,
    IEmailVerificationService emailVerificationService) : PageModel
{
    public string? Username { get; private set; }

    public bool IsEmailConfirmed { get; private set; }

    [BindProperty]
    [Required]
    [EmailAddress]
    public string? Email { get; set; }

    [BindProperty]
    [Phone]
    [Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task OnGetAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);
        Load(user);
        Email = user.Email;
        PhoneNumber = user.PhoneNumber;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);
        if (!ModelState.IsValid)
        {
            Load(user);
            return Page();
        }

        if (Email != user.Email)
        {
            var setEmailResult = await userManager.SetEmailAsync(user, Email);
            if (!setEmailResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred setting email for user with ID '{user.Id}'.");
            }
        }

        if (PhoneNumber != user.PhoneNumber)
        {
            var setPhoneResult = await userManager.SetPhoneNumberAsync(user, PhoneNumber);
            if (!setPhoneResult.Succeeded)
            {
                throw new ApplicationException($"Unexpected error occurred setting phone number for user with ID '{user.Id}'.");
            }
        }

        StatusMessage = "Your profile has been updated";
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSendVerificationEmailAsync()
    {
        var user = await userManager.GetRequiredUserAsync(User);
        if (!ModelState.IsValid)
        {
            Load(user);
            return Page();
        }

        await emailVerificationService.SendVerificationEmailAsync(user, Url, Request.Scheme);

        StatusMessage = "Verification email sent. Please check your email.";
        return RedirectToPage();
    }

    private void Load(ApplicationUser user)
    {
        Username = user.UserName;
        IsEmailConfirmed = user.EmailConfirmed;
    }
}
