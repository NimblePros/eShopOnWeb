using Microsoft.AspNetCore.Mvc;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.Web.Interfaces;

public interface IEmailVerificationService
{
    /// <summary>
    /// Generates an email confirmation token for the user and emails them a link to confirm their address.
    /// </summary>
    Task SendVerificationEmailAsync(ApplicationUser user, IUrlHelper urlHelper, string scheme);
}
