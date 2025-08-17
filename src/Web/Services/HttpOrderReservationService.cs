using System.Net.Http.Json;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.Web.Services;

public sealed class HttpOrderReservationService(HttpClient http, ILogger<HttpOrderReservationService> logger)
    : IOrderReservationService
{
    private readonly HttpClient _http = http;
    private readonly ILogger<HttpOrderReservationService> _logger = logger;

    public async Task NotifyAsync(Order order, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            orderId = order.Id,
            items = order.OrderItems.Select(i => new { itemId = i.Id, quantity = i.Units })
        };

        using var resp = await _http.PostAsJsonAsync("/api/reserve", payload, cancellationToken);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("OrderItemsReserver failed: {Status} {Body}", (int)resp.StatusCode, body);
        }
    }
}
