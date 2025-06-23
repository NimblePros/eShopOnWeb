using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Infrastructure.Services;

public class LoggerEmailSender(IAppLogger<LoggerEmailSender> logger): IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string message)
    {
        logger.LogInformation("to: {email}, subject: {emailSubject}, message: {emailMessage}", email, subject, message);
        return Task.CompletedTask;
    }
}
