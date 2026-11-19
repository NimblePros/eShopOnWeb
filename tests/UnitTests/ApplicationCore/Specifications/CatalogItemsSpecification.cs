using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.UnitTests.Builders;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class CatalogItemsSpecification
{
    [Fact]
    public void MatchesSpecificCatalogItem()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogItemsSpecification(1);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal(1, Assert.Single(result).Id);
    }

    [Fact]
    public void MatchesAllCatalogItems()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogItemsSpecification(1, 3);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal([1, 3], result.Select(i => i.Id));
    }

    [Fact]
    public void MatchesNothingGivenNoIds()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogItemsSpecification();

        Assert.Empty(spec.Evaluate(GetTestCollection()));
    }

    [Fact]
    public void IgnoresUnknownIds()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogItemsSpecification(3, 99);

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal(3, Assert.Single(result).Id);
    }

    private static List<CatalogItem> GetTestCollection() =>
    [
        new CatalogItem(1, 1, "Item 1 description", "Item 1", 1.5m, "Item1Uri").WithId(1),
        new CatalogItem(3, 3, "Item 3 description", "Item 3", 3.5m, "Item3Uri").WithId(3)
    ];
}
