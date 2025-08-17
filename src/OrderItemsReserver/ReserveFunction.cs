using System.Net;
using System.Text.Json;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderItemsReserver.Models;

namespace OrderItemsReserver;

public class ReserveFunction
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IOptions<OrdersOptions> _options;
    private readonly ILogger<ReserveFunction> _logger;

    public ReserveFunction(
        BlobServiceClient blobServiceClient,
        IOptions<OrdersOptions> options,
        ILogger<ReserveFunction> logger)
    {
        _blobServiceClient = blobServiceClient;
        _options = options;
        _logger = logger;
    }

    [Function("Reserve")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req)
    {
        try
        {
            var request = await JsonSerializer.DeserializeAsync<OrderRequest>(
                req.Body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (request is null || request.Items is null || request.Items.Count == 0)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload. Expected { orderId, items[{itemId, quantity}] }.");
                return bad;
            }

            var minimal = new
            {
                orderId = request.OrderId,
                items = request.Items.Select(i => new { itemId = i.ItemId, quantity = i.Quantity })
            };

            var bytes = JsonSerializer.SerializeToUtf8Bytes(minimal, new JsonSerializerOptions { WriteIndented = true });
            var fileName = $"order-{request.OrderId}-{DateTimeOffset.UtcNow:yyyyMMddHHmmssfff}.json";

            var container = _blobServiceClient.GetBlobContainerClient(_options.Value.ContainerName);
            await container.CreateIfNotExistsAsync(PublicAccessType.None);

            using var ms = new MemoryStream(bytes);
            await container.UploadBlobAsync(fileName, ms);

            var ok = req.CreateResponse(HttpStatusCode.Created);
            await ok.WriteStringAsync(fileName);
            return ok;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reserve items.");
            var err = req.CreateResponse(HttpStatusCode.InternalServerError);
            await err.WriteStringAsync("Failed to process order.");
            return err;
        }
    }
}
