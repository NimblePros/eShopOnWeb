using Microsoft.eShopWeb.ApplicationCore.Entities;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.CatalogItemTests;

public class UpdateDetails
{
    private const string OriginalName = "Name";
    private const string OriginalDescription = "Description";
    private const decimal OriginalPrice = 1.00m;

    private static CatalogItem CreateCatalogItem() =>
        new(catalogTypeId: 1, catalogBrandId: 1, OriginalDescription, OriginalName, OriginalPrice, "images\\products\\original.png");

    [Fact]
    public void UpdatesNameDescriptionAndPrice()
    {
        var catalogItem = CreateCatalogItem();

        catalogItem.UpdateDetails(new CatalogItem.CatalogItemDetails("New name", "New description", 9.99m));

        Assert.Equal("New name", catalogItem.Name);
        Assert.Equal("New description", catalogItem.Description);
        Assert.Equal(9.99m, catalogItem.Price);
    }

    [Theory]
    [InlineData(null, "Description", 1.00)]
    [InlineData("", "Description", 1.00)]
    [InlineData("Name", null, 1.00)]
    [InlineData("Name", "", 1.00)]
    [InlineData("Name", "Description", 0)]
    [InlineData("Name", "Description", -0.01)]
    public void ThrowsAndLeavesItemUnchangedGivenInvalidDetails(string? name, string? description, double price)
    {
        var catalogItem = CreateCatalogItem();

        Assert.ThrowsAny<ArgumentException>(() =>
            catalogItem.UpdateDetails(new CatalogItem.CatalogItemDetails(name, description, (decimal)price)));

        Assert.Equal(OriginalName, catalogItem.Name);
        Assert.Equal(OriginalDescription, catalogItem.Description);
        Assert.Equal(OriginalPrice, catalogItem.Price);
    }
}
