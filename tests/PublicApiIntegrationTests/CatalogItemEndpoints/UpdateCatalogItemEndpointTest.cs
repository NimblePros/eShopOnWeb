using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.CatalogItemEndpoints;

[TestClass]
public class UpdateCatalogItemEndpointTest
{
    private const string Route = "api/catalog-items";
    private const int NonExistentId = 9999;

    [TestMethod]
    public async Task ReturnsUnauthorizedGivenNoToken()
    {
        var client = ProgramTest.NewClient;

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(id: 1)));

        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var client = HttpClientHelper.GetNormalUserClient();

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(id: 1)));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsForbiddenGivenAdminUserToken()
    {
        var client = HttpClientHelper.GetAdminClient();

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(id: 1)));

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsNotFoundGivenNonExistentIdAndProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();

        var response = await client.PutAsync(Route, JsonContent.Create(BuildRequest(id: NonExistentId)));

        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    public async Task UpdatesItemGivenValidChangesAndProductManagerUserToken()
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var itemId = await CreateCatalogItemAsync(client);
        var request = BuildRequest(itemId);

        var response = await client.PutAsync(Route, JsonContent.Create(request));

        response.EnsureSuccessStatusCode();
        var updated = (await response.Content.ReadAsStringAsync()).FromJson<UpdateCatalogItemResponse>()!.CatalogItem;
        AssertMatchesRequest(request, updated);

        var getResponse = await client.GetAsync($"{Route}/{itemId}");
        getResponse.EnsureSuccessStatusCode();
        var persisted = (await getResponse.Content.ReadAsStringAsync()).FromJson<GetByIdCatalogItemResponse>()!.CatalogItem;
        AssertMatchesRequest(request, persisted);
    }

    private static void AssertMatchesRequest(UpdateCatalogItemRequest expected, CatalogItemDto actual)
    {
        Assert.AreEqual(expected.Id, actual.Id);
        Assert.AreEqual(expected.Name, actual.Name);
        Assert.AreEqual(expected.Description, actual.Description);
        Assert.AreEqual(expected.Price, actual.Price);
        Assert.AreEqual(expected.CatalogBrandId, actual.CatalogBrandId);
        Assert.AreEqual(expected.CatalogTypeId, actual.CatalogTypeId);
    }

    private static UpdateCatalogItemRequest BuildRequest(int id) => new()
    {
        Id = id,
        CatalogBrandId = 2,
        CatalogTypeId = 3,
        Name = $"updated name {Guid.NewGuid()}",
        Description = "updated description",
        Price = 9.87m
    };

    // Creates a dedicated item so the update doesn't alter seeded data other tests rely on.
    private static async Task<int> CreateCatalogItemAsync(HttpClient client)
    {
        var createRequest = new CreateCatalogItemRequest
        {
            CatalogBrandId = 1,
            CatalogTypeId = 1,
            Name = $"item to update {Guid.NewGuid()}",
            Description = "original description",
            Price = 1.23m
        };
        var response = await client.PostAsync(Route, JsonContent.Create(createRequest));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<CreateCatalogItemResponse>()!.CatalogItem.Id;
    }
}
