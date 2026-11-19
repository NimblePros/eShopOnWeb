namespace Microsoft.eShopWeb.PublicApi.RoleManagementEndpoints;

/// <summary>
/// The public shape of a role. Keeps Identity storage fields such as NormalizedName and
/// ConcurrencyStamp out of API responses.
/// </summary>
public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
}
