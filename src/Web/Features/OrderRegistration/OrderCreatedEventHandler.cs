using System.Net.Http;
using System.Text;
using Azure.Core;
using Azure.Identity;
using MediatR;
using Microsoft.eShopWeb.ApplicationCore.Events.OrderRegistration;
using Microsoft.eShopWeb.Infrastructure.Services;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.Web.Features.OrderRegistration;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly ServiceBusService _serviceBusService;
    private readonly HttpClient _httpClient;
    private readonly string _clientId;
    private readonly string _registerOrderForDeliveryUrl;
    private readonly string _ordersFunctionScope;

    public OrderCreatedEventHandler(ServiceBusService serviceBus, HttpClient httpClient, IConfiguration configuration)
    {
        _serviceBusService = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _clientId = configuration["ManagedIdentityClientId"]!;
        _registerOrderForDeliveryUrl = configuration["RegisterOrderForDeliveryUrl"]!;
        _ordersFunctionScope = configuration["OrdersFunctionScope"]!;
    }

    public async Task Handle(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken)
    {
        var payload = JsonConvert.SerializeObject(orderCreatedEvent);

        await RegisterOrderAsync(payload);
        await RegisterOrderForDeliveryAsync(payload);
    }

    private async Task RegisterOrderAsync(string payload)
    {
        await _serviceBusService.SendMessageAsync(payload);
    }

    private async Task RegisterOrderForDeliveryAsync(string payload)
    {
        var credential = new ManagedIdentityCredential(_clientId);

        var token = await credential.GetTokenAsync(
            new TokenRequestContext(
                new[] { _ordersFunctionScope }));

        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);

        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        await _httpClient.PostAsync(_registerOrderForDeliveryUrl, content);
    }
}
