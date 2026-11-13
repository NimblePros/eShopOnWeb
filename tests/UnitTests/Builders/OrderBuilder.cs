using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.UnitTests.Builders;

public class OrderBuilder
{
    private Order _order;
    public string TestBuyerId => "12345";
    public int TestCatalogItemId => 234;
    public string TestProductName => "Test Product Name";
    public string TestPictureUri => "http://test.com/image.jpg";
    public decimal TestUnitPrice = 1.23m;
    public int TestUnits = 3;
    // Owned entity instances can't be shared between OrderItems (EF Core rejects it),
    // so hand out a new instance each time, the same way OrderService does.
    public CatalogItemOrdered CreateCatalogItemOrdered() => new(TestCatalogItemId, TestProductName, TestPictureUri);

    public OrderBuilder()
    {
        _order = WithDefaultValues();
    }

    public Order Build()
    {
        return _order;
    }

    public Order WithDefaultValues()
    {
        var orderItem = new OrderItem(CreateCatalogItemOrdered(), TestUnitPrice, TestUnits);
        var itemList = new List<OrderItem>() { orderItem };
        _order = new Order(TestBuyerId, new AddressBuilder().WithDefaultValues(), itemList);
        return _order;
    }

    public Order WithNoItems()
    {
        _order = new Order(TestBuyerId, new AddressBuilder().WithDefaultValues(), new List<OrderItem>());
        return _order;
    }

    public Order WithItems(List<OrderItem> items)
    {
        _order = new Order(TestBuyerId, new AddressBuilder().WithDefaultValues(), items);
        return _order;
    }
}
