using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Specifications;

/// <summary>
/// Loads a single order, with its items, only if it belongs to <c>buyerId</c>.
/// </summary>
public class CustomerOrderWithItemsByIdSpec : Specification<Order>
{
    public CustomerOrderWithItemsByIdSpec(string buyerId, int orderId)
    {
        Query
            .Where(order => order.Id == orderId && order.BuyerId == buyerId)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.ItemOrdered);
    }
}
