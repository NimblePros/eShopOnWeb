using System.Net.Http;
using System.Text;
using Azure;
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
    private readonly string _registerOrderForDeliveryUrl;
    private readonly string _ordersFunctionScope;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(ServiceBusService serviceBus, HttpClient httpClient, IConfiguration configuration, ILogger<OrderCreatedEventHandler> logger)
    {
        _serviceBusService = serviceBus ?? throw new ArgumentNullException(nameof(serviceBus));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _registerOrderForDeliveryUrl = configuration["RegisterOrderForDeliveryUrl"]!;
        _ordersFunctionScope = configuration["OrdersFunctionScope"]!;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        var content = new StringContent(payload, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(_registerOrderForDeliveryUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorMessage = $"Failed to register order for delivery. Status Code: {response.StatusCode}, Reason {response.ReasonPhrase}, Error: {errorContent}";
            _logger.LogError(errorMessage);

            throw new HttpRequestException(errorMessage);
        }
    }
}
