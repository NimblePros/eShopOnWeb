using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.Infrastructure.Services;

public class ServiceBusService
{
    private readonly string _fullyQualifiedNamespace;
    private readonly string _queueName;
    private readonly string _clientId;

    public ServiceBusService(IConfiguration configuration)
    {
        _fullyQualifiedNamespace = configuration["ServiceBusConnection:FullyQualifiedNamespace"]!;
        _queueName = configuration["ServiceBusConnection:QueueName"]!;
        _clientId = configuration["ManagedIdentityClientId"]!;
    }

    public async Task SendMessageAsync(string messageBody)
    {
        var credential = new ManagedIdentityCredential(_clientId);

        await using (var client = new ServiceBusClient(_fullyQualifiedNamespace, credential))
        {
            var sender = client.CreateSender(_queueName);
            var message = new ServiceBusMessage(messageBody);

            await sender.SendMessageAsync(message);
        }
    }
}
