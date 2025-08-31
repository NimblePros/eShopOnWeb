using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Microsoft.eShopWeb.Web.Delivery;

public record OrderItemDto(
    [property: JsonPropertyName("catalogItemId")] int CatalogItemId,
    [property: JsonPropertyName("productName")] string ProductName,
    [property: JsonPropertyName("unitPrice")] decimal UnitPrice,
    [property: JsonPropertyName("quantity")] int Quantity);

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
    [JsonProperty(PropertyName = "id")]
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
}
