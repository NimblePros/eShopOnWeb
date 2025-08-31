using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DeliveryOrderProcessor.Models;

public class OrderItemDto
{
    [JsonPropertyName("catalogItemId")]
    public int CatalogItemId { get; set; }

    [JsonPropertyName("productName")]
    public string? ProductName { get; set; }

    [JsonPropertyName("unitPrice")]
    public decimal? UnitPrice { get; set; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; set; }
}


public record ShippingAddressDto(
    [property: JsonPropertyName("fullName")] string FullName,
    [property: JsonPropertyName("phone")] string Phone,
    [property: JsonPropertyName("line1")] string Line1,
    [property: JsonPropertyName("line2")] string? Line2,
    [property: JsonPropertyName("city")] string City,
    [property: JsonPropertyName("state")] string State,
    [property: JsonPropertyName("zip")] string Zip,
    [property: JsonPropertyName("country")] string Country);

public class DeliveryOrderDto
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = default!;

    [JsonPropertyName("orderId")]
    public int OrderId { get; set; }

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = default!;

    [JsonPropertyName("createdUtc")]
    public DateTime CreatedUtc { get; set; }

    [JsonPropertyName("finalPrice")]
    public decimal FinalPrice { get; set; }

    [JsonPropertyName("shippingAddress")]
    public ShippingAddressDto ShippingAddress { get; set; } = default!;

    [JsonPropertyName("items")]
    public List<OrderItemDto> Items { get; set; } = new();

    [JsonPropertyName("status")]
    public string Status { get; set; } = "new";
}
