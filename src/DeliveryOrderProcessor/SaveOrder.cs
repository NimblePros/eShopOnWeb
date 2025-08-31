using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using DeliveryOrderProcessor.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Cosmos;
using System;

public class SaveOrder
{
    private readonly CosmosClient _cosmos;
    private readonly ILogger<SaveOrder> _logger;
    private const string DbName = "deliverydb";
    private const string ContainerName = "orders";

    public SaveOrder(CosmosClient cosmos, ILogger<SaveOrder> logger)
    {
        _cosmos = cosmos;
        _logger = logger;
    }

    [Function("SaveOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders")] HttpRequestData req)
    {
        try
        {
            var dto = await JsonSerializer.DeserializeAsync<DeliveryOrderDto>(
                req.Body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto is null)
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Invalid payload: body is null");
                return bad;
            }

            if (string.IsNullOrWhiteSpace(dto.Id))
            {
                dto.Id = Guid.NewGuid().ToString();
            }

            _logger.LogInformation("SaveOrder: id={Id}, orderId={OrderId}, user={UserId}", dto.Id, dto.OrderId, dto.UserId);

            var container = _cosmos.GetContainer(DbName, ContainerName);
            var cosmosResp = await container.CreateItemAsync(dto, new PartitionKey(dto.Id));

            var created = req.CreateResponse(HttpStatusCode.Created);
            await created.WriteStringAsync($"stored; ru:{cosmosResp.RequestCharge}");
            return created;
        }
        catch (CosmosException cex)
        {
            _logger.LogError(cex, "Cosmos error: {Message}", cex.Message);
            var resp = req.CreateResponse((HttpStatusCode)cex.StatusCode);
            await resp.WriteStringAsync(cex.Message);
            return resp;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled SaveOrder error: {Message}", ex.Message);
            var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
            await resp.WriteStringAsync($"error: {ex.Message}");
            return resp;
        }
    }
}
