using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Microsoft.eShopWeb.Web.Pages.Manage;

public static class ManageNavPages
{
    public static string ActivePageKey => "ActivePage";

    public static string Index => "Index";

    public static string ChangePassword => "ChangePassword";

    public static string ExternalLogins => "ExternalLogins";

    public static string TwoFactorAuthentication => "TwoFactorAuthentication";

    public static string IndexNavClass(ViewContext viewContext) => PageNavClass(viewContext, Index);

    public static string ChangePasswordNavClass(ViewContext viewContext) => PageNavClass(viewContext, ChangePassword);

    public static string ExternalLoginsNavClass(ViewContext viewContext) => PageNavClass(viewContext, ExternalLogins);

    public static string TwoFactorAuthenticationNavClass(ViewContext viewContext) => PageNavClass(viewContext, TwoFactorAuthentication);

    public static string PageNavClass(ViewContext viewContext, string page) =>
        IsActive(viewContext, page) ? "active" : string.Empty;

    /// <summary>
    /// "page" for the active nav link so screen readers announce it; null omits the attribute.
    /// </summary>
    public static string? AriaCurrent(ViewContext viewContext, string page) =>
        IsActive(viewContext, page) ? "page" : null;

    private static bool IsActive(ViewContext viewContext, string page)
    {
        var activePage = viewContext.ViewData[ActivePageKey] as string;
        return string.Equals(activePage, page, StringComparison.OrdinalIgnoreCase);
    }

    public static void AddActivePage(this ViewDataDictionary viewData, string activePage) => viewData[ActivePageKey] = activePage;
}
