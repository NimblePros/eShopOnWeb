using Microsoft.Extensions.Hosting;

namespace Microsoft.eShopWeb.Infrastructure;

public static class HostEnvironmentExtensions
{
    /// <summary>
    /// Environments where sample conveniences (demo users, default passwords, development JWT key) are allowed.
    /// </summary>
    public static bool IsLocalDevelopment(this IHostEnvironment environment)
        => environment.IsDevelopment()
        || environment.IsEnvironment("Docker")
        || environment.IsEnvironment("Testing");
}
