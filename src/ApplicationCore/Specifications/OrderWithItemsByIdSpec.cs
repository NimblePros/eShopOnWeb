using Ardalis.Specification;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Specifications;

/// <summary>
/// Loads an order by id with no ownership check. To load an order on behalf of a user, use
/// <see cref="CustomerOrderWithItemsByIdSpec"/> instead.
/// </summary>
public class OrderWithItemsByIdSpec : Specification<Order>
{
    public OrderWithItemsByIdSpec(int orderId)
    {
        Query
            .Where(order => order.Id == orderId)
            .Include(o => o.OrderItems)
            .ThenInclude(i => i.ItemOrdered);
    }
}
