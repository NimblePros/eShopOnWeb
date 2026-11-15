using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.eShopWeb;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests.CatalogItemEndpoints;

[TestClass]
public class CatalogItemListPagedEndpoint
{
    [TestMethod]
    public async Task ReturnsFirst10CatalogItems()
    {
        var client = ProgramTest.NewClient;
        var response = await client.GetAsync("/api/catalog-items?pageSize=10");
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<ListPagedCatalogItemResponse>();

        Assert.AreEqual(10, model!.CatalogItems.Count());
    }

    [TestMethod]
    public async Task ReturnsCorrectCatalogItemsGivenPageIndex1()
    {

        var pageSize = 10;
        var pageIndex = 1;

        var client = ProgramTest.NewClient;
        var response = await client.GetAsync($"/api/catalog-items");
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync();
        var model = stringResponse.FromJson<ListPagedCatalogItemResponse>();
        var totalItem = model!.CatalogItems.Count();

        var response2 = await client.GetAsync($"/api/catalog-items?pageSize={pageSize}&pageIndex={pageIndex}");
        response.EnsureSuccessStatusCode();
        var stringResponse2 = await response2.Content.ReadAsStringAsync();
        var model2 = stringResponse2.FromJson<ListPagedCatalogItemResponse>();

        var totalExpected = totalItem - (pageSize * pageIndex);

        Assert.AreEqual(totalExpected, model2!.CatalogItems.Count());
    }

    [TestMethod]
    public async Task MapsEveryCatalogItemFieldTheSameAsGetById()
    {
        var client = ProgramTest.NewClient;
        var listResponse = await client.GetAsync("/api/catalog-items?pageSize=1");
        listResponse.EnsureSuccessStatusCode();
        var listed = (await listResponse.Content.ReadAsStringAsync())
            .FromJson<ListPagedCatalogItemResponse>()!.CatalogItems.Single();

        var byIdResponse = await client.GetAsync($"/api/catalog-items/{listed.Id}");
        byIdResponse.EnsureSuccessStatusCode();
        var expected = (await byIdResponse.Content.ReadAsStringAsync())
            .FromJson<GetByIdCatalogItemResponse>()!.CatalogItem;

        Assert.AreEqual(expected.Id, listed.Id);
        Assert.AreEqual(expected.Name, listed.Name);
        Assert.AreEqual(expected.Description, listed.Description);
        Assert.AreEqual(expected.Price, listed.Price);
        Assert.AreEqual(expected.PictureUri, listed.PictureUri);
        Assert.AreEqual(expected.CatalogTypeId, listed.CatalogTypeId);
        Assert.AreEqual(expected.CatalogBrandId, listed.CatalogBrandId);
        Assert.IsFalse(string.IsNullOrEmpty(listed.Name));
        Assert.IsTrue(listed.Price > 0);
    }

    [TestMethod]
    public async Task ReturnsAllSeededItemsWhenPageSizeIsOmitted()
    {
        var client = ProgramTest.NewClient;
        var response = await client.GetAsync("/api/catalog-items");
        response.EnsureSuccessStatusCode();
        var model = (await response.Content.ReadAsStringAsync()).FromJson<ListPagedCatalogItemResponse>();

        // The seeded catalog is smaller than MaxPageSize, so the default page holds every item.
        Assert.IsTrue(model!.CatalogItems.Count > 0);
        Assert.IsTrue(model.CatalogItems.Count <= ListPagedCatalogItemRequest.MaxPageSize);
        Assert.AreEqual(1, model.PageCount);
    }

    [TestMethod]
    public async Task ReturnsOkGivenMaxPageSize()
    {
        var client = ProgramTest.NewClient;
        var response = await client.GetAsync($"/api/catalog-items?pageSize={ListPagedCatalogItemRequest.MaxPageSize}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    [DataRow("pageSize=0")]
    [DataRow("pageSize=-1")]
    [DataRow("pageSize=101")]
    [DataRow("pageSize=1000000")]
    [DataRow("pageSize=10&pageIndex=-1")]
    [DataRow("pageSize=100&pageIndex=100000000")]
    [DataRow("pageIndex=-5")]
    public async Task ReturnsBadRequestGivenInvalidPaging(string query)
    {
        var client = ProgramTest.NewClient;
        var response = await client.GetAsync($"/api/catalog-items?{query}");

        Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [TestMethod]
    [DataRow("catalog-items")]
    [DataRow("catalog-brands")]
    [DataRow("catalog-types")]
    [DataRow("catalog-items/1")]
    public async Task SuccessFullMutipleParallelCall(string endpointName)
    {
        var client = ProgramTest.NewClient;
        var tasks = new List<Task<HttpResponseMessage>>();

        for (int i = 0; i < 100; i++)
        {
            var task = client.GetAsync($"/api/{endpointName}");
            tasks.Add(task);
        }
        await Task.WhenAll(tasks.ToList());
        var totalKO = tasks.Count(t => t.Result.StatusCode != HttpStatusCode.OK);

        Assert.AreEqual(0, totalKO);
    }
}
