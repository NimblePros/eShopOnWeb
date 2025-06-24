using System;
using System.Text.Json.Serialization;
using NServiceBus;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
public class OrderCreatedEvent :IMessage
{
    public int Id { get; init; }
    public string BuyerId { get; init; }
    public DateTimeOffset OrderDate { get; init; }
    public decimal Total { get; init; }
    public OrderCreatedEvent() { }

    public OrderCreatedEvent(int orderId, string buyerId, DateTimeOffset orderDate, decimal orderTotal) {
        Id = orderId;
        BuyerId = buyerId;
        OrderDate = orderDate;
        Total = orderTotal;
    }
}
