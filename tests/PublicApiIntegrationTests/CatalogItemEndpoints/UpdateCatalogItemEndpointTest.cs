using System;
using System.Globalization;
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
    private const int SeededItemId = 1;

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

    [TestMethod]
    [DataRow(nameof(UpdateCatalogItemRequest.Name), "")]
    [DataRow(nameof(UpdateCatalogItemRequest.Name), null)]
    [DataRow(nameof(UpdateCatalogItemRequest.Name), "this name is longer than the fifty character limit!")]
    [DataRow(nameof(UpdateCatalogItemRequest.Description), "")]
    [DataRow(nameof(UpdateCatalogItemRequest.Description), null)]
    [DataRow(nameof(UpdateCatalogItemRequest.Price), "0")]
    [DataRow(nameof(UpdateCatalogItemRequest.Price), "-1")]
    [DataRow(nameof(UpdateCatalogItemRequest.CatalogBrandId), "0")]
    [DataRow(nameof(UpdateCatalogItemRequest.CatalogTypeId), "0")]
    [DataRow(nameof(UpdateCatalogItemRequest.Id), "0")]
    public async Task ReturnsBadRequestGivenInvalidField(string field, string? value)
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var request = BuildRequest(SeededItemId);
        SetField(request, field, value);

        var response = await client.PutAsync(Route, JsonContent.Create(request));

        await AssertBadRequestForFieldAsync(response, field);
    }

    [TestMethod]
    public async Task ReturnsBadRequestGivenNonExistentBrand()
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var itemId = await CreateCatalogItemAsync(client);
        var request = BuildRequest(itemId);
        request.CatalogBrandId = NonExistentId;

        var response = await client.PutAsync(Route, JsonContent.Create(request));

        await AssertBadRequestForFieldAsync(response, nameof(UpdateCatalogItemRequest.CatalogBrandId));
    }

    [TestMethod]
    public async Task ReturnsBadRequestGivenNonExistentType()
    {
        var client = HttpClientHelper.GetProductManagerClient();
        var itemId = await CreateCatalogItemAsync(client);
        var request = BuildRequest(itemId);
        request.CatalogTypeId = NonExistentId;

        var response = await client.PutAsync(Route, JsonContent.Create(request));

        await AssertBadRequestForFieldAsync(response, nameof(UpdateCatalogItemRequest.CatalogTypeId));
    }

    private static void SetField(UpdateCatalogItemRequest request, string field, string? value)
    {
        switch (field)
        {
            case nameof(UpdateCatalogItemRequest.Name): request.Name = value!; break;
            case nameof(UpdateCatalogItemRequest.Description): request.Description = value!; break;
            case nameof(UpdateCatalogItemRequest.Price): request.Price = decimal.Parse(value!, CultureInfo.InvariantCulture); break;
            case nameof(UpdateCatalogItemRequest.CatalogBrandId): request.CatalogBrandId = int.Parse(value!, CultureInfo.InvariantCulture); break;
            case nameof(UpdateCatalogItemRequest.CatalogTypeId): request.CatalogTypeId = int.Parse(value!, CultureInfo.InvariantCulture); break;
            case nameof(UpdateCatalogItemRequest.Id): request.Id = int.Parse(value!, CultureInfo.InvariantCulture); break;
            default: throw new ArgumentOutOfRangeException(nameof(field), field, null);
        }
    }

    private static async Task AssertBadRequestForFieldAsync(HttpResponseMessage response, string field)
    {
        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(body.ToLowerInvariant(), $"\"{field.ToLowerInvariant()}\"");
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
            Name = $"to update {Guid.NewGuid()}",
            Description = "original description",
            Price = 1.23m
        };
        var response = await client.PostAsync(Route, JsonContent.Create(createRequest));
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadAsStringAsync()).FromJson<CreateCatalogItemResponse>()!.CatalogItem.Id;
    }
}
