using System;
using Ardalis.GuardClauses;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

public class BasketItem : BaseEntity
{
    /// <summary>
    /// The largest quantity a single basket line may hold.
    /// </summary>
    public const int MaxQuantity = 999;

    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public int CatalogItemId { get; private set; }
    public int BasketId { get; private set; }

    public BasketItem(int catalogItemId, int quantity, decimal unitPrice)
    {
        CatalogItemId = catalogItemId;
        UnitPrice = unitPrice;
        SetQuantity(quantity);
    }

    // For EF Core materialization. Rows saved before MaxQuantity existed can hold larger
    // quantities, and loading them must not run the public constructor's guard.
    private BasketItem() { }

    public void AddQuantity(int quantity)
    {
        // Bounding the addition by the remaining headroom keeps the result <= MaxQuantity
        // without overflowing. A legacy row already above the max has no headroom.
        Guard.Against.OutOfRange(quantity, nameof(quantity), 0, Math.Max(0, MaxQuantity - Quantity));

        Quantity += quantity;
    }

    public void SetQuantity(int quantity)
    {
        Guard.Against.OutOfRange(quantity, nameof(quantity), 0, MaxQuantity);

        Quantity = quantity;
    }

    /// <summary>
    /// Adds <paramref name="quantity"/>, capping the result at <see cref="MaxQuantity"/>
    /// instead of throwing.
    /// </summary>
    public void AddQuantityUpToMax(int quantity)
    {
        Guard.Against.Negative(quantity, nameof(quantity));

        // long arithmetic: a legacy Quantity near int.MaxValue must not wrap negative.
        Quantity = (int)Math.Min(MaxQuantity, (long)Quantity + quantity);
    }
}
