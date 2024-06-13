using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Constants;
using Microsoft.eShopWeb.Web.Extensions;
using Microsoft.eShopWeb.Web.Services;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Caching.Memory;

namespace Microsoft.eShopWeb.Web.Pages.Admin;

[Authorize(Roles = "Administrators")]
public class IndexModel : PageModel
{
    public IndexModel()
    {

    }
}
