using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Microsoft.eShopWeb.Web.Pages.SavedItems;

[Authorize]
public class IndexModel : PageModel
{
    public void OnGet()
    {
    }
}
