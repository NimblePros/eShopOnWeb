using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Microsoft.eShopWeb.ApplicationCore.Events.OrderRegistration;

public class OrderCreatedEvent : INotification
{
    public int Id { get; set; }
    public string BuyerId { get; set; }
    public DateTimeOffset OrderDate { get; set; }
    public AddressDto ShipToAddress { get; set; }
    public IEnumerable<OrderItemDto> OrderItems { get; set; }
}
