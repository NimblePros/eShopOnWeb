using System.Text.RegularExpressions;
using Ardalis.GuardClauses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Web.Interfaces;
using Microsoft.eShopWeb.Web.ViewModels;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

public partial class IndexModel : PageModel
{
    private readonly IBasketService _basketService;
    private readonly IBasketViewModelService _basketViewModelService;
    private readonly IRepository<CatalogItem> _itemRepository;

    public IndexModel(IBasketService basketService,
        IBasketViewModelService basketViewModelService,
        IRepository<CatalogItem> itemRepository)
    {
        _basketService = basketService;
        _basketViewModelService = basketViewModelService;
        _itemRepository = itemRepository;
    }

    public const string StatusMessageKey = nameof(StatusMessage);

    public static readonly string MaxQuantityReachedMessage =
        $"Your basket can hold at most {BasketItem.MaxQuantity} of each item.";

    /// <summary>Fallback for a quantity error that carries no message of its own.</summary>
    public static readonly string InvalidQuantityMessage =
        $"Quantity must be between 0 and {BasketItem.MaxQuantity}.";

    public const string NotSavedMessage = "Your basket was not updated. Correct the quantities below and try again.";

    public const string UpdateFailedMessage = "Your basket could not be updated. Please try again.";

    public BasketViewModel BasketModel { get; set; } = new BasketViewModel();

    [TempData]
    public string? StatusMessage { get; set; }

    /// <summary>
    /// After a rejected update, the quantity text the shopper posted for each line, keyed by
    /// basket item id, so the form shows what they typed instead of the saved quantities.
    /// </summary>
    public IReadOnlyDictionary<int, string> PostedQuantities { get; private set; } = new Dictionary<int, string>();

    /// <summary>
    /// After a rejected update, the validation error for each invalid line, keyed by basket item id.
    /// </summary>
    public IReadOnlyDictionary<int, string> QuantityErrors { get; private set; } = new Dictionary<int, string>();

    public async Task OnGet()
    {
        BasketModel = await _basketViewModelService.GetOrCreateBasketForUser(GetOrSetBasketCookieAndUserName());
    }

    public async Task<IActionResult> OnPost(CatalogItemViewModel productDetails)
    {
        if (productDetails?.Id == null)
        {
            return RedirectToPage("/Index");
        }

        var item = await _itemRepository.GetByIdAsync(productDetails.Id);
        if (item == null)
        {
            return RedirectToPage("/Index");
        }

        var username = GetOrSetBasketCookieAndUserName();
        try
        {
            await _basketService.AddItemToBasket(username, productDetails.Id, item.Price);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Adding a single unit only fails when the line is already at BasketItem.MaxQuantity.
            StatusMessage = MaxQuantityReachedMessage;
        }

        return RedirectToPage();
    }

    public async Task OnPostUpdate(IEnumerable<BasketItemViewModel> items)
    {
        var basketView = await _basketViewModelService.GetOrCreateBasketForUser(GetOrSetBasketCookieAndUserName());
        if (!ModelState.IsValid)
        {
            // Show the basket as it is, alongside the validation errors and the values the shopper typed.
            BasketModel = basketView;
            CapturePostedQuantities(basketView);
            return;
        }

        var updateModel = items.ToDictionary(b => b.Id.ToString(), b => b.Quantity);
        var basket = await _basketService.SetQuantities(basketView.Id, updateModel);
        BasketModel = await _basketViewModelService.Map(basket);
    }

    private void CapturePostedQuantities(BasketViewModel basket)
    {
        var productNames = basket.Items.ToDictionary(item => item.Id, item => item.ProductName);
        var posted = new Dictionary<int, string>();
        var errors = new Dictionary<int, string>();

        foreach (var (key, entry) in ModelState)
        {
            var match = PostedQuantityKey().Match(key);
            // Key lines by basket item id, not form index: the index only matches the rendered row
            // while the basket is unchanged. Ignore lines that aren't in this shopper's basket.
            if (!match.Success
                || !ModelState.TryGetValue($"{match.Groups["prefix"].Value}.Id", out var idEntry)
                || !int.TryParse(idEntry.AttemptedValue, out var id)
                || !productNames.ContainsKey(id)
                // A tampered form can repeat a line id; keep the first so its value and error stay paired.
                || posted.ContainsKey(id))
            {
                continue;
            }

            posted[id] = entry.AttemptedValue ?? string.Empty;
            var error = entry.Errors.FirstOrDefault(e => !string.IsNullOrEmpty(e.ErrorMessage));
            if (error != null)
            {
                errors[id] = error.ErrorMessage;
            }
            else if (entry.Errors.Count > 0)
            {
                errors[id] = InvalidQuantityMessage;
            }
        }

        // The per-line messages sit next to their inputs; the summary says nothing was saved and
        // names each invalid product.
        if (errors.Count > 0)
        {
            ModelState.AddModelError(string.Empty, NotSavedMessage);
        }
        foreach (var item in basket.Items.Where(item => errors.ContainsKey(item.Id)))
        {
            ModelState.AddModelError(string.Empty, $"{item.ProductName}: {errors[item.Id]}");
        }

        // Any other invalid field (for example a tampered line id) would otherwise reject the update silently.
        if (errors.Count == 0)
        {
            ModelState.AddModelError(string.Empty, UpdateFailedMessage);
        }

        PostedQuantities = posted;
        QuantityErrors = errors;
    }

    [GeneratedRegex(@"^(?<prefix>items\[\d+\])\.Quantity$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PostedQuantityKey();

    private string GetOrSetBasketCookieAndUserName()
    {
        Guard.Against.Null(Request.HttpContext.User.Identity, nameof(Request.HttpContext.User.Identity));
        string? userName = null;

        if (Request.HttpContext.User.Identity.IsAuthenticated)
        {
            Guard.Against.Null(Request.HttpContext.User.Identity.Name, nameof(Request.HttpContext.User.Identity.Name));
            return Request.HttpContext.User.Identity.Name!;
        }

        if (Request.Cookies.ContainsKey(Constants.BASKET_COOKIENAME))
        {
            userName = Request.Cookies[Constants.BASKET_COOKIENAME];

            if (!Request.HttpContext.User.Identity.IsAuthenticated)
            {
                if (!Guid.TryParse(userName, out var _))
                {
                    userName = null;
                }
            }
        }
        if (userName != null) return userName;

        userName = Guid.NewGuid().ToString();
        var cookieOptions = new CookieOptions { IsEssential = true };
        cookieOptions.Expires = DateTime.Today.AddYears(10);
        Response.Cookies.Append(Constants.BASKET_COOKIENAME, userName, cookieOptions);

        return userName;
    }
}
