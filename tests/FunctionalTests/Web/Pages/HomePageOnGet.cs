using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web.Pages;

[Collection(WebTestCollection.Name)]
public class HomePageOnGet(TestApplication factory)
{
    public HttpClient Client { get; } = factory.CreateClient();

    [Fact]
    public async Task ReturnsHomePageWithProductListing()
    {
        // Arrange & Act
        var response = await Client.GetAsync("/", TestContext.Current.CancellationToken);
        response.EnsureSuccessStatusCode();
        var stringResponse = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        // Assert
        Assert.Contains(".NET Bot Black Sweatshirt", stringResponse);
    }
}
