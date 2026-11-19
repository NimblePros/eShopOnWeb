using System;
using System.Collections.Generic;

namespace Microsoft.eShopWeb.ApplicationCore.Exceptions;

/// <summary>
/// Thrown at checkout when basket lines refer to catalog items that no longer exist, for example
/// because a product was deleted after the shopper added it to their basket.
/// </summary>
public class UnavailableBasketItemsException : Exception
{
    public UnavailableBasketItemsException(IReadOnlyCollection<int> catalogItemIds)
        : base($"Basket contains catalog items that are no longer available: {string.Join(", ", catalogItemIds)}")
    {
        CatalogItemIds = catalogItemIds;
    }

    public IReadOnlyCollection<int> CatalogItemIds { get; }
}
