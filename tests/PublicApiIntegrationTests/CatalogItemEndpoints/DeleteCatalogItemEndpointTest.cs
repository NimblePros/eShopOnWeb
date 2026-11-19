using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.CatalogItemEndpoints;

[TestClass]
public class DeleteCatalogItemEndpointTest
{
    private const string Route = "api/catalog-items";

    [TestMethod]
    public async Task ReturnsSuccessGivenValidIdAndProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var id = await CreateItemAsync(client);

        var response = await client.DeleteAsync($"{Route}/{id}");

        Assert.AreEqual(HttpStatusCode.NoContent, response.StatusCode);
    }

    [TestMethod]
    public async Task RemovesItemSoLaterGetAndDeleteReturnNotFound()
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var id = await CreateItemAsync(client);
        (await client.DeleteAsync($"{Route}/{id}")).EnsureSuccessStatusCode();

        var getResponse = await client.GetAsync($"{Route}/{id}");
        var secondDeleteResponse = await client.DeleteAsync($"{Route}/{id}");

        Assert.AreEqual(HttpStatusCode.NotFound, getResponse.StatusCode);
        Assert.AreEqual(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenInvalidIdAndProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var response = await client.DeleteAsync($"{Route}/0");

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsUnauthorizedGivenNoToken()
    {
        var id = await CreateItemAsync(HttpClientHelper.GetProductManagerClient());

        var response = await ProgramTest.NewClient.DeleteAsync($"{Route}/{id}");

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        await AssertItemStillExistsAsync(id);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var id = await CreateItemAsync(HttpClientHelper.GetProductManagerClient());

        var response = await HttpClientHelper.GetNormalUserClient().DeleteAsync($"{Route}/{id}");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        await AssertItemStillExistsAsync(id);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenAdminUserToken()
    {
        // Only Product Managers manage the catalog; Administrators manage users and roles.
        var id = await CreateItemAsync(HttpClientHelper.GetProductManagerClient());

        var response = await HttpClientHelper.GetAdminClient().DeleteAsync($"{Route}/{id}");

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        await AssertItemStillExistsAsync(id);
    }

    private static async Task<int> CreateItemAsync(HttpClient client)
    {
        var response = await client.PostAsJsonAsync(Route, new CreateCatalogItemRequest
        {
            CatalogBrandId = 1,
            CatalogTypeId = 1,
            Description = "to be deleted",
            Name = $"delete {Guid.NewGuid():N}"[..30],
            Price = 1.23m
        });
        response.EnsureSuccessStatusCode();
        var created = (await response.Content.ReadAsStringAsync()).FromJson<CreateCatalogItemResponse>();
        return created!.CatalogItem.Id;
    }

    private static async Task AssertItemStillExistsAsync(int id)
    {
        var response = await ProgramTest.NewClient.GetAsync($"{Route}/{id}");
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }
}
