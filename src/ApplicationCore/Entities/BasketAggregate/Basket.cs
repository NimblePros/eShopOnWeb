using System.Collections.Generic;
using System.Linq;
using Ardalis.GuardClauses;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.BasketAggregate;

public class Basket : BaseEntity, IAggregateRoot
{
    public string BuyerId { get; private set; }
    private readonly List<BasketItem> _items = new List<BasketItem>();
    public IReadOnlyCollection<BasketItem> Items => _items.AsReadOnly();

    public int TotalItems => _items.Sum(i => i.Quantity);


    public Basket(string buyerId)
    {
        BuyerId = buyerId;
    }

    public void AddItem(int catalogItemId, decimal unitPrice, int quantity = 1)
    {
        if (!Items.Any(i => i.CatalogItemId == catalogItemId))
        {
            _items.Add(new BasketItem(catalogItemId, quantity, unitPrice));
            return;
        }
        var existingItem = Items.First(i => i.CatalogItemId == catalogItemId);
        existingItem.AddQuantity(quantity);
    }

    /// <summary>
    /// Adds an item like <see cref="AddItem"/>, but caps the line at
    /// <see cref="BasketItem.MaxQuantity"/> instead of throwing when the total would exceed it.
    /// Used when merging one basket into another.
    /// </summary>
    public void MergeItem(int catalogItemId, decimal unitPrice, int quantity)
    {
        Guard.Against.Negative(quantity, nameof(quantity));

        var item = _items.FirstOrDefault(i => i.CatalogItemId == catalogItemId);
        if (item == null)
        {
            item = new BasketItem(catalogItemId, 0, unitPrice);
            _items.Add(item);
        }
        item.AddQuantityUpToMax(quantity);
    }

    public void RemoveEmptyItems()
    {
        _items.RemoveAll(i => i.Quantity == 0);
    }

    public void RemoveItemsForCatalogItems(IEnumerable<int> catalogItemIds)
    {
        var idsToRemove = catalogItemIds.ToHashSet();
        _items.RemoveAll(i => idsToRemove.Contains(i.CatalogItemId));
    }

    public void SetNewBuyerId(string buyerId)
    {
        BuyerId = buyerId;
    }
}
