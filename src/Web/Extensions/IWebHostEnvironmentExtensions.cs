namespace Microsoft.eShopWeb.Web.Extensions;

public static class IWebHostEnvironmentExtensions
{
    public static bool IsDocker(this IWebHostEnvironment environment)
        => environment.EnvironmentName == "Docker";
}
