using Ardalis.GuardClauses;
using Mediator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.Web.Features.OrderDetails;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages.Order;

[Authorize]
public class DetailModel(IMediator mediator) : PageModel
{
    public OrderDetailViewModel Order { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(int orderId)
    {
        Guard.Against.Null(User?.Identity?.Name, nameof(User.Identity.Name));
        var order = await mediator.Send(new GetOrderDetails(User.Identity.Name, orderId));

        if (order == null)
        {
            return BadRequest("No such order found for this user.");
        }

        Order = order;
        return Page();
    }
}
