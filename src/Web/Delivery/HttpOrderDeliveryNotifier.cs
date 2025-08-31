using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;

namespace Microsoft.eShopWeb.Web.Delivery;

public class HttpOrderDeliveryNotifier : IOrderDeliveryNotifier
{
    private readonly HttpClient _http;
    private readonly ILogger<HttpOrderDeliveryNotifier> _logger;
    private readonly string _functionBaseUrl;
    private readonly string _functionCode;

    public HttpOrderDeliveryNotifier(HttpClient http,
                                     ILogger<HttpOrderDeliveryNotifier> logger,
                                     IConfiguration cfg)
    {
        _http = http;
        _logger = logger;
        _functionBaseUrl = cfg["Delivery:FunctionBaseUrl"]
            ?? throw new InvalidOperationException("Missing Delivery:FunctionBaseUrl");
        _functionCode = cfg["Delivery:FunctionCode"]
            ?? throw new InvalidOperationException("Missing Delivery:FunctionCode");
    }

    public async Task NotifyAsync(DeliveryOrderDto order, CancellationToken ct = default)
    {
        // Ensure Cosmos-compatible id is present
        if (string.IsNullOrWhiteSpace(order.Id))
        {
            order.Id = Guid.NewGuid().ToString();
        }

        var url = $"{_functionBaseUrl.TrimEnd('/')}/api/orders?code={_functionCode}";
        var res = await _http.PostAsJsonAsync(url, order, ct);
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            _logger.LogError("Delivery notify failed: {Status} {Body}", res.StatusCode, body);
        }
        else
        {
            _logger.LogInformation("Delivery notify ok for order {OrderId}", order.OrderId);
        }
    }
}
