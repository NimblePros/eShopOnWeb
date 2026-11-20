using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Hosting;

namespace Microsoft.eShopWeb.Infrastructure.Services;

/// <summary>
/// Sample <see cref="IEmailSender"/> that writes emails to the log instead of sending them.
/// </summary>
/// <remarks>
/// Email bodies hold account tokens (email confirmation, password reset links), and recipient
/// addresses are PII. The full email is logged only in local development, where developers need
/// the links to finish those flows. Everywhere else only the subject and a masked recipient are logged.
/// </remarks>
public class LoggerEmailSender(IAppLogger<LoggerEmailSender> logger, IHostEnvironment environment) : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        if (environment.IsLocalDevelopment())
        {
            logger.LogInformation("to: {email}, subject: {emailSubject}, message: {emailMessage}", email, subject, message);
        }
        else
        {
            logger.LogInformation("Email not sent (no email sender configured). to: {maskedEmail}, subject: {emailSubject}",
                MaskEmail(email), subject);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Keeps the first character of the local part and the domain: "jane@example.com" becomes "j***@example.com".
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return "***";
        }

        var atIndex = email.LastIndexOf('@');
        if (atIndex <= 0 || atIndex == email.Length - 1)
        {
            return "***";
        }

        return $"{email[0]}***{email[atIndex..]}";
    }
}
