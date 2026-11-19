using Microsoft.eShopWeb.ApplicationCore.Entities;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Specifications;

public class CatalogItemNameSpecification
{
    [Fact]
    public void MatchesItemWithExactName()
    {
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogItemNameSpecification("Mug");

        var result = spec.Evaluate(GetTestCollection()).ToList();

        Assert.Equal("Mug", Assert.Single(result).Name);
    }

    [Theory]
    [InlineData("mug")]
    [InlineData("Mu")]
    [InlineData("Mug ")]
    [InlineData("")]
    public void MatchesNothingGivenNameThatIsNotAnExactMatch(string name)
    {
        // In memory the comparison is ordinal; the database collation decides case sensitivity.
        var spec = new eShopWeb.ApplicationCore.Specifications.CatalogItemNameSpecification(name);

        Assert.Empty(spec.Evaluate(GetTestCollection()));
    }

    private static List<CatalogItem> GetTestCollection() =>
    [
        new CatalogItem(1, 1, "Description", "Mug", 1m, "uri"),
        new CatalogItem(1, 1, "Description", "Mug Large", 2m, "uri"),
        new CatalogItem(1, 1, "Description", "T-Shirt", 3m, "uri")
    ];
}
