using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate.Events;
using NServiceBus;

namespace Microsoft.eShopWeb.ApplicationCore.Configuration;
public static class NServiceBusConfiguration
{
    public static EndpointConfiguration GetNServiceBusConfiguration()
    {
        var endpointConfiguration = new EndpointConfiguration("orders-worker");
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        var transport = endpointConfiguration.UseTransport<LearningTransport>();

        transport.Routing().RouteToEndpoint(
          typeof(OrderCreatedEvent),
          "orders-worker");

        endpointConfiguration.SendFailedMessagesTo("error");
        endpointConfiguration.AuditProcessedMessagesTo("audit");

        return endpointConfiguration;
    }
}
