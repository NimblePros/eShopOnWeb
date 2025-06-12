using System.Diagnostics;
using System.Security.Claims;

namespace Microsoft.eShopWeb.Web;

public sealed class UserContextEnrichmentMiddleware(
    RequestDelegate next,
    ILogger<UserContextEnrichmentMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        string? userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            Activity.Current?.SetTag("user.id", userId);

            var data = new Dictionary<string, object>
            {
                ["UserId"] = userId
            };

            using (logger.BeginScope(data))
            {
                await next(context);
            }
        }
        else
        {
            await next(context);
        }
    }
}
