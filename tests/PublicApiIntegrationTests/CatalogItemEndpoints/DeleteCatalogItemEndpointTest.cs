using System.Net;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.CatalogItemEndpoints;

[TestClass]
public class DeleteCatalogItemEndpointTest
{
    [TestMethod]
    public async Task ReturnsSuccessGivenValidIdAndProductManagerUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.DeleteAsync("api/catalog-items/12");
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<DeleteCatalogItemResponse>();

        Assert.AreEqual("Deleted", model!.Status);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidIdAndProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var response = await client.DeleteAsync("api/catalog-items/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
