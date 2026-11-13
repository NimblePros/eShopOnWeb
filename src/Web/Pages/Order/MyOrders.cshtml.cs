using Ardalis.GuardClauses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Web.Features.MyOrders;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages.Order;

[Authorize]
public class MyOrdersModel(IMediator mediator) : PageModel
{
    public IEnumerable<OrderViewModel> Orders { get; private set; } = [];

    public async Task OnGetAsync()
    {
        Guard.Against.Null(User?.Identity?.Name, nameof(User.Identity.Name));
        Orders = await mediator.Send(new GetMyOrders(User.Identity.Name));
    }
}
