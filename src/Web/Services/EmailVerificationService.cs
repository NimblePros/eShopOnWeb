using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Services;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;

    public EmailVerificationService(UserManager<ApplicationUser> userManager, IEmailSender emailSender)
    {
        _userManager = userManager;
        _emailSender = emailSender;
    }

    public async Task SendVerificationEmailAsync(ApplicationUser user, IUrlHelper urlHelper, string scheme)
    {
        Guard.Against.Null(user);
        Guard.Against.Null(urlHelper);

        var email = user.Email;
        if (string.IsNullOrEmpty(email))
        {
            throw new ApplicationException($"No email associated with user '{user.UserName}'.");
        }

        var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = urlHelper.EmailConfirmationLink(user.Id, code, scheme);
        Guard.Against.Null(callbackUrl);

        await _emailSender.SendEmailConfirmationAsync(email, callbackUrl);
    }
}
