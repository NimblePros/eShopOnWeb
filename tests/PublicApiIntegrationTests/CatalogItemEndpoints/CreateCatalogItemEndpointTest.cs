using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorShared.Models;
using Microsoft.eShopWeb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PublicApiIntegrationTests.Helpers;

namespace PublicApiIntegrationTests.AuthEndpoints;

[TestClass]
public class CreateCatalogItemEndpointTest
{
    private int _testBrandId = 1;
    private int _testTypeId = 2;
    private string _testDescription = "test description";
    private string _testName = "test name";
    private decimal _testPrice = 1.23m;


    [TestMethod]
    public async Task ReturnsForbiddenGivenNormalUserToken()
    {
        var jsonContent = GetValidNewItemJson();
        var client = HttpClientHelper.GetNormalUserClient();
        var response = await client.PostAsync("api/catalog-items", jsonContent);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }


    [TestMethod]
    public async Task ReturnsForbiddenGivenAdminUserToken()
    {
        var jsonContent = GetValidNewItemJson();
        var client = HttpClientHelper.GetAdminClient();
        var response = await client.PostAsync("api/catalog-items", jsonContent);

        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    public async Task ReturnsSuccessGivenValidNewItemAndProductManagerUserToken()
    {
        var jsonContent = GetValidNewItemJson();
        var client = HttpClientHelper.GetProductManagerClient();
        var response = await client.PostAsync("api/catalog-items", jsonContent);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<CreateCatalogItemResponse>();

        Assert.AreEqual(_testBrandId, model!.CatalogItem.CatalogBrandId);
        Assert.AreEqual(_testTypeId, model.CatalogItem.CatalogTypeId);
        Assert.AreEqual(_testDescription, model.CatalogItem.Description);
        Assert.AreEqual(_testName, model.CatalogItem.Name);
        Assert.AreEqual(_testPrice, model.CatalogItem.Price);
    }

    [TestMethod]
    public async Task ReturnsConflictGivenDuplicateItemName()
    {
        var duplicateName = $"duplicate {Guid.NewGuid()}";
        var client = HttpClientHelper.GetProductManagerClient();
        var firstResponse = await client.PostAsync("api/catalog-items", GetValidNewItemJson(duplicateName));
        firstResponse.EnsureSuccessStatusCode();

        var secondResponse = await client.PostAsync("api/catalog-items", GetValidNewItemJson(duplicateName));

        Assert.AreEqual(HttpStatusCode.Conflict, secondResponse.StatusCode);
    }

    [TestMethod]
    [DataRow(nameof(CreateCatalogItemRequest.Name), "")]
    [DataRow(nameof(CreateCatalogItemRequest.Name), null)]
    [DataRow(nameof(CreateCatalogItemRequest.Name), "this name is longer than the fifty character limit!")]
    [DataRow(nameof(CreateCatalogItemRequest.Description), "")]
    [DataRow(nameof(CreateCatalogItemRequest.Description), null)]
    [DataRow(nameof(CreateCatalogItemRequest.Price), "0")]
    [DataRow(nameof(CreateCatalogItemRequest.Price), "-1")]
    [DataRow(nameof(CreateCatalogItemRequest.CatalogBrandId), "0")]
    [DataRow(nameof(CreateCatalogItemRequest.CatalogTypeId), "0")]
    [DataRow(nameof(CreateCatalogItemRequest.CatalogBrandId), "9999")]
    [DataRow(nameof(CreateCatalogItemRequest.CatalogTypeId), "9999")]
    public async Task ReturnsBadRequestGivenInvalidField(string field, string? value)
    {
        var request = BuildValidRequest($"invalid {Guid.NewGuid()}");
        switch (field)
        {
            case nameof(CreateCatalogItemRequest.Name): request.Name = value!; break;
            case nameof(CreateCatalogItemRequest.Description): request.Description = value!; break;
            case nameof(CreateCatalogItemRequest.Price): request.Price = decimal.Parse(value!, CultureInfo.InvariantCulture); break;
            case nameof(CreateCatalogItemRequest.CatalogBrandId): request.CatalogBrandId = int.Parse(value!, CultureInfo.InvariantCulture); break;
            case nameof(CreateCatalogItemRequest.CatalogTypeId): request.CatalogTypeId = int.Parse(value!, CultureInfo.InvariantCulture); break;
            default: throw new ArgumentOutOfRangeException(nameof(field), field, null);
        }
        var client = HttpClientHelper.GetProductManagerClient();

        var response = await client.PostAsync("api/catalog-items", ToJson(request));

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        StringAssert.Contains(body.ToLowerInvariant(), $"\"{field.ToLowerInvariant()}\"");
    }

    private StringContent GetValidNewItemJson(string? name = null) => ToJson(BuildValidRequest(name));

    private static StringContent ToJson(CreateCatalogItemRequest request) =>
        new(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

    private CreateCatalogItemRequest BuildValidRequest(string? name = null)
    {
        return new CreateCatalogItemRequest()
        {
            CatalogBrandId = _testBrandId,
            CatalogTypeId = _testTypeId,
            Description = _testDescription,
            Name = name ?? _testName,
            Price = _testPrice
        };
    }
}
