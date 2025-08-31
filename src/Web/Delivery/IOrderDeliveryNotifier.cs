namespace Microsoft.eShopWeb.Web.Delivery;

public interface IOrderDeliveryNotifier
{
    Task NotifyAsync(DeliveryOrderDto order, CancellationToken ct = default);
}
