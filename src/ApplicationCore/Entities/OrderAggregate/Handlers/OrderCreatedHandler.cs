using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.Extensions.Logging;

namespace Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Handlers;

public class OrderCreatedHandler(ILogger<OrderCreatedHandler> logger, IEmailSender emailSender) : INotificationHandler<OrderCreatedEvent>
{
    public async Task Handle(OrderCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order #{orderId} placed: ", domainEvent.Order.Id);

        await emailSender.SendEmailAsync("to@test.com",
                                         "Order Created",
                                         $"Order with id {domainEvent.Order.Id} was created.");
    }
}
