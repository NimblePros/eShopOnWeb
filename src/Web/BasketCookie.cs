namespace Microsoft.eShopWeb.Web;

/// <summary>
/// Reads and writes the cookie that identifies an anonymous shopper's basket. The cookie holds a
/// random GUID that stands in for the basket owner's username until the shopper signs in.
/// </summary>
public static class BasketCookie
{
    /// <summary>
    /// Returns the anonymous basket id from the request's basket cookie, or null when there is no
    /// cookie or its value is not a GUID. Rejecting other values stops a shopper from setting the
    /// cookie to someone else's username (an email address) and reaching that user's basket.
    /// </summary>
    public static string? GetAnonymousId(HttpRequest request)
    {
        var value = request.Cookies[Constants.BASKET_COOKIENAME];
        return Guid.TryParse(value, out _) ? value : null;
    }

    /// <summary>
    /// Returns the request's anonymous basket id, or creates a new one and writes it to the
    /// response's basket cookie.
    /// </summary>
    public static string GetOrCreateAnonymousId(HttpContext httpContext)
    {
        var anonymousId = GetAnonymousId(httpContext.Request);
        if (anonymousId != null)
        {
            return anonymousId;
        }

        anonymousId = Guid.NewGuid().ToString();
        httpContext.Response.Cookies.Append(Constants.BASKET_COOKIENAME, anonymousId, CreateOptions());
        return anonymousId;
    }

    /// <summary>The options every basket cookie is written with.</summary>
    public static CookieOptions CreateOptions() => new()
    {
        Expires = DateTimeOffset.UtcNow.AddYears(10),
        HttpOnly = true,
        Secure = true,
        // Lax so the basket survives a shopper following a link from another site. Strict would
        // hide the cookie on that first request. See docs/decisions/2026-11-19-cookie-samesite-policy.md.
        SameSite = SameSiteMode.Lax,
        // The basket can't work without it, so it doesn't wait for cookie consent.
        IsEssential = true,
    };
}
