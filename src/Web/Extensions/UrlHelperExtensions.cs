namespace Microsoft.AspNetCore.Mvc;

public static class UrlHelperExtensions
{
    /// <summary>
    /// Absolute link to the Identity area's ConfirmEmail page for the given user and token.
    /// </summary>
    public static string? EmailConfirmationLink(this IUrlHelper urlHelper, string userId, string code, string scheme)
    {
        return urlHelper.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId, code },
            protocol: scheme);
    }
}
