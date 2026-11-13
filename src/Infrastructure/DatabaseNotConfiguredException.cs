using System;

namespace Microsoft.eShopWeb.Infrastructure;

/// <summary>
/// Thrown when the app is set up to use SQL Server but has no connection string for a database.
/// Startup lets this propagate (instead of logging it as a seeding error) so the app stops with
/// instructions rather than running and failing on every request.
/// </summary>
public class DatabaseNotConfiguredException(string connectionName)
    : InvalidOperationException(
        $"Connection string '{connectionName}' is not configured. Run the solution through the eShopWeb.AppHost " +
        "(.NET Aspire) project, which starts SQL Server in a container and supplies it, set " +
        $"'ConnectionStrings:{connectionName}' yourself (for example with user secrets), or set " +
        "'UseOnlyInMemoryDatabase' to true.")
{
    public string ConnectionName { get; } = connectionName;
}
