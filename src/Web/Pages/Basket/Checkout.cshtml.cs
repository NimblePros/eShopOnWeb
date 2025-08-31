using System.Linq;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Exceptions;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Identity;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.Delivery;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

[Authorize]
public class CheckoutModel : PageModel
{
    private readonly IBasketService _basketService;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IOrderService _orderService;
    private readonly IBasketViewModelService _basketViewModelService;
    private readonly IAppLogger<CheckoutModel> _logger;
    private readonly IOrderDeliveryNotifier _deliveryNotifier;

    private string? _username = null;

    public CheckoutModel(
        IBasketService basketService,
        IBasketViewModelService basketViewModelService,
        SignInManager<ApplicationUser> signInManager,
        IOrderService orderService,
        IAppLogger<CheckoutModel> logger,
        IOrderDeliveryNotifier deliveryNotifier)
    {
        _basketService = basketService;
        _signInManager = signInManager;
        _orderService = orderService;
        _basketViewModelService = basketViewModelService;
        _logger = logger;
        _deliveryNotifier = deliveryNotifier;
    }

    public BasketViewModel BasketModel { get; private set; } = new();

    [BindProperty, Required] public string FullName { get; set; } = default!;
    [BindProperty, Required] public string PhoneNumber { get; set; } = default!;
    [BindProperty, Required] public string Address1 { get; set; } = default!;
    [BindProperty] public string? Address2 { get; set; }
    [BindProperty, Required] public string City { get; set; } = default!;
    [BindProperty, Required] public string State { get; set; } = default!;
    [BindProperty, Required] public string Country { get; set; } = default!;
    [BindProperty, Required] public string ZipCode { get; set; } = default!;

    public async Task OnGet()
    {
        await SetBasketModelAsync();
    }

    public async Task<IActionResult> OnPostAsync(IEnumerable<BasketItemViewModel> items, CancellationToken ct)
    {
        try
        {
            await SetBasketModelAsync();

            if (!ModelState.IsValid)
            {
                // Return an error page, not JSON 400
                return Page();
            }

            // 1) update quantities
            var updateModel = items.ToDictionary(b => b.Id.ToString(), b => b.Quantity);
            await _basketService.SetQuantities(BasketModel.Id, updateModel);

            // 2) create an order in SQL and take its id
            var address = new Address(Address1, City, State, Country, ZipCode);
            var createdOrderId = await _orderService.CreateOrderAsync(BasketModel.Id, address);

            // 3) DTO for delivery service
            var finalPrice = BasketModel.Items.Sum(i => i.UnitPrice * i.Quantity);
            var dto = new DeliveryOrderDto
            {
                Id = $"ord_{createdOrderId}",     // or Guid.NewGuid().ToString()
                OrderId = createdOrderId,         // the actual order id
                UserId = _signInManager.UserManager.GetUserId(User)!,
                CreatedUtc = DateTime.UtcNow,
                FinalPrice = finalPrice,
                ShippingAddress = new ShippingAddressDto(
                    FullName, PhoneNumber, Address1, Address2, City, State, ZipCode, Country),
                Items = BasketModel.Items
                    .Select(i => new OrderItemDto(i.CatalogItemId, i.ProductName ?? string.Empty, i.UnitPrice, i.Quantity))
                    .ToList()
            };

            // 4) Do NOT block checkout if the external service is temporarily unavailable
            try
            {
                await _deliveryNotifier.NotifyAsync(dto, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Delivery notify failed for order {OrderId}. Exception: {Exception}", createdOrderId, ex);
            }

            // 5) empty the recycle garbage can and go to Success
            await _basketService.DeleteBasketAsync(BasketModel.Id);
        }
        catch (EmptyBasketOnCheckoutException ex)
        {
            _logger.LogWarning("Empty basket on checkout. Exception: {Exception}", ex);
            return RedirectToPage("/Basket/Index");
        }

        return RedirectToPage("Success");
    }

    private async Task SetBasketModelAsync()
    {
        Guard.Against.Null(User?.Identity?.Name, nameof(User.Identity.Name));

        if (_signInManager.IsSignedIn(User))
        {
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(User.Identity!.Name!);
        }
        else
        {
            GetOrSetBasketCookieAndUserName();
            BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(_username!);
        }
    }

    private void GetOrSetBasketCookieAndUserName()
    {
        if (Request.Cookies.TryGetValue(Constants.BASKET_COOKIENAME, out var existing))
        {
            _username = existing;
        }

        if (_username != null) return;

        _username = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(10) };
        Response.Cookies.Append(Constants.BASKET_COOKIENAME, _username, cookieOptions);
    }
}
