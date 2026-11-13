using System.Linq;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.CatalogBrandEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.CatalogBrandEndpoints;

[TestClass]
public class CatalogBrandListEndpointTest
{
    [TestMethod]
    public async Task ReturnsAllSeededBrandsWithNameMappedFromBrand()
    {
        var client = ProgramTest.NewClient;
        var response = await client.GetAsync("/api/catalog-brands");
        response.EnsureSuccessStatusCode();
        var model = (await response.Content.ReadAsStringAsync()).FromJson<ListCatalogBrandsResponse>();

        CollectionAssert.AreEquivalent(
            new[] { "Azure", ".NET", "Visual Studio", "SQL Server", "Other" },
            model!.CatalogBrands.Select(b => b.Name).ToArray());
        Assert.IsTrue(model.CatalogBrands.All(b => b.Id > 0));
    }
}
