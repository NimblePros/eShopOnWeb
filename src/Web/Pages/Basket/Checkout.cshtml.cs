using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Interfaces;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

[Authorize]
public class CheckoutModel : PageModel
{
    public const string UnavailableItemsRemovedMessage =
        "Some items in your basket are no longer available and were removed. Please review your basket and check out again.";


    private readonly IBasketService _basketService;
    private readonly IOrderService _orderService;
    private readonly IBasketViewModelService _basketViewModelService;
    private readonly IAppLogger<CheckoutModel> _logger;

    public CheckoutModel(IBasketService basketService,
        IBasketViewModelService basketViewModelService,
        IOrderService orderService,
        IAppLogger<CheckoutModel> logger)
    {
        _basketService = basketService;
        _orderService = orderService;
        _basketViewModelService = basketViewModelService;
        _logger = logger;
    }

    public BasketViewModel BasketModel { get; set; } = new BasketViewModel();

    public async Task OnGet()
    {
        await SetBasketModelAsync();
    }

    public async Task<IActionResult> OnPost(IEnumerable<BasketItemViewModel> items)
    {
        try
        {
            await SetBasketModelAsync();

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var updateModel = items.ToDictionary(b => b.Id.ToString(), b => b.Quantity);
            await _basketService.SetQuantities(BasketModel.Id, updateModel);
            await _orderService.CreateOrderAsync(BasketModel.Id, new Address("123 Main St.", "Kent", "OH", "United States", "44240"));
            await _basketService.DeleteBasketAsync(BasketModel.Id);
        }
        catch (EmptyBasketOnCheckoutException emptyBasketOnCheckoutException)
        {
            //Redirect to Empty Basket page
            _logger.LogWarning(emptyBasketOnCheckoutException.Message);
            return RedirectToPage("/Basket/Index");
        }
        catch (UnavailableBasketItemsException unavailableBasketItemsException)
        {
            _logger.LogWarning(unavailableBasketItemsException.Message);
            await _basketService.RemoveItemsForCatalogItemsAsync(BasketModel.Id, unavailableBasketItemsException.CatalogItemIds);
            TempData[IndexModel.StatusMessageKey] = UnavailableItemsRemovedMessage;
            return RedirectToPage("/Basket/Index");
        }

        return RedirectToPage("Success");
    }

    private async Task SetBasketModelAsync()
    {
        // Checkout requires a signed-in user ([Authorize] here and AuthorizePage in Program.cs), so the
        // basket always belongs to that user. Supporting guest checkout would mean reading the anonymous
        // basket id through BasketCookie, which rejects values that aren't GUIDs.
        Guard.Against.Null(User?.Identity?.Name, nameof(User.Identity.Name));
        BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(User.Identity.Name);
    }
}
