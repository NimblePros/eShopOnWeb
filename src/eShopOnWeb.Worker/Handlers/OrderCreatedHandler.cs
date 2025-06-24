using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
using Microsoft.Extensions.Logging;
using NServiceBus;

namespace eShopOnWeb.Worker.Handlers;

    public class OrderCreatedEventHandler(ILogger<OrderCreatedEventHandler> logger)
  : IHandleMessages<OrderCreatedEvent>
    {
        private readonly ILogger<OrderCreatedEventHandler> _logger = logger;

        public Task Handle(OrderCreatedEvent message, IMessageHandlerContext context)
        {
            _logger.LogInformation("{OrderDate} - Received {EventName} - Order {OrderId} from buyer {BuyerId}: {OrderTotal:C}",
              nameof(OrderCreatedEvent),
              message.OrderDate,message.Id,message.BuyerId,message.Total);
            return Task.CompletedTask;
        }
    }

