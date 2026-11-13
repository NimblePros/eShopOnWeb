using Microsoft.eShopWeb.ApplicationCore.Entities;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class CatalogFilterPaginatedSpecification
{
    [Fact]
    public void ReturnsAllCatalogItems()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogFilterPaginatedSpecification(0, 10, null, null);

        var result = spec.Evaluate(GetTestCollection());

        Assert.NotNull(result);
        Assert.Equal(4, result.ToList().Count);
    }

    [Fact]
    public void Returns2CatalogItemsWithSameBrandAndTypeId()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogFilterPaginatedSpecification(0, 10, 1, 1);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.NotNull(result);
        Assert.Equal(2, result.ToList().Count);
    }

    [Fact]
    public void ReturnsAllCatalogItemsGivenTakeOfZero()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogFilterPaginatedSpecification(0, 0, null, null);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public void ReturnsRequestedPageOfCatalogItems()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogFilterPaginatedSpecification(1, 2, null, null);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal(["Item 1.5", "Item 2"], result.Select(i => i.Name));
    }

    [Theory]
    [InlineData(2, null, "Item 2")]
    [InlineData(null, 3, "Item 3")]
    public void FiltersByBrandOrTypeWhenOnlyOneIsProvided(int? brandId, int? typeId, string expectedName)
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogFilterPaginatedSpecification(0, 10, brandId, typeId);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal(expectedName, Assert.Single(result).Name);
    }

    private List<CatalogItem> GetTestCollection()
    {
        var catalogItemList = new List<CatalogItem>();

        catalogItemList.Add(new CatalogItem(1, 1, "Item 1", "Item 1", 1.00m, "TestUri1"));
        catalogItemList.Add(new CatalogItem(1, 1, "Item 1.5", "Item 1.5", 1.50m, "TestUri1"));
        catalogItemList.Add(new CatalogItem(2, 2, "Item 2", "Item 2", 2.00m, "TestUri2"));
        catalogItemList.Add(new CatalogItem(3, 3, "Item 3", "Item 3", 3.00m, "TestUri3"));

        return catalogItemList;
    }
}
