using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages.SavedItems;

[Collection("Sequential")]
public class SavedItemsPageTests : IClassFixture<TestApplication>
{
    public SavedItemsPageTests(TestApplication factory)
    {
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    public HttpClient Client { get; }

    [Fact]
    public async Task Anonymous_Get_RedirectsToLogin()
    {
        var response = await Client.GetAsync("/SavedItems", TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.StartsWith("/Account/Login", response.Headers.Location!.OriginalString);
    }
}
