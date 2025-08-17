namespace OrderItemsReserver.Models;

public sealed class OrderRequest
{
    public int OrderId { get; set; }
    public List<OrderItemDto> Items { get; set; } = [];
}

public sealed class OrderItemDto
{
    public int ItemId { get; set; }
    public int Quantity { get; set; }
}

public sealed class OrdersOptions
{
    public string ContainerName { get; set; } = "orders";
}
