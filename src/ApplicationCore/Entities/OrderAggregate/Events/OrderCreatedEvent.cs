using Mediator;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;

public class OrderCreatedEvent(Order order) : INotification
{
    public Order Order { get; init; } = order;
}
