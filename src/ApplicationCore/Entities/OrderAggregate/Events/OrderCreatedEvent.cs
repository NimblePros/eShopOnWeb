using Ardalis.SharedKernel;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
public class OrderCreatedEvent(Order order) : DomainEventBase
{
    public Order Order { get; init; } = order;
}
