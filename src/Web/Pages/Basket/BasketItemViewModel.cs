using System.ComponentModel.DataAnnotations;
using Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

namespace Microsoft.eShopWeb.Web.Pages.Basket;

public class BasketItemViewModel
{
    public int Id { get; set; }
    public int CatalogItemId { get; set; }
    public string? ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OldUnitPrice { get; set; }

    [Range(0, BasketItem.MaxQuantity, ErrorMessage = "Quantity must be between {1} and {2}.")]
    public int Quantity { get; set; }

    public string? PictureUrl { get; set; }
}
