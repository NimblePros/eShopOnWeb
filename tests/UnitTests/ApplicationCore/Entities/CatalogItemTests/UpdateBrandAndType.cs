using Microsoft.eShopWeb.ApplicationCore.Entities;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.CatalogItemTests;

public class UpdateBrandAndType
{
    private static CatalogItem CreateCatalogItem() =>
        new(catalogTypeId: 1, catalogBrandId: 1, "Description", "Name", 1.00m, "images\\products\\original.png");

    [Fact]
    public void UpdateBrandSetsBrandId()
    {
        var catalogItem = CreateCatalogItem();

        catalogItem.UpdateBrand(5);

        Assert.Equal(5, catalogItem.CatalogBrandId);
    }

    [Fact]
    public void UpdateBrandThrowsGivenZero()
    {
        var catalogItem = CreateCatalogItem();

        Assert.Throws<ArgumentException>(() => catalogItem.UpdateBrand(0));
        Assert.Equal(1, catalogItem.CatalogBrandId);
    }

    [Fact]
    public void UpdateTypeSetsTypeId()
    {
        var catalogItem = CreateCatalogItem();

        catalogItem.UpdateType(7);

        Assert.Equal(7, catalogItem.CatalogTypeId);
    }

    [Fact]
    public void UpdateTypeThrowsGivenZero()
    {
        var catalogItem = CreateCatalogItem();

        Assert.Throws<ArgumentException>(() => catalogItem.UpdateType(0));
        Assert.Equal(1, catalogItem.CatalogTypeId);
    }
}
