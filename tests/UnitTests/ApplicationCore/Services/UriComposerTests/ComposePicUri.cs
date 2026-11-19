using Microsoft.eShopWeb.ApplicationCore.Services;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Services.UriComposerTests;

public class ComposePicUri
{
    private const string Placeholder = "http://catalogbaseurltobereplaced";

    [Fact]
    public void ReplacesPlaceholderWithCatalogBaseUrl()
    {
        var composer = new UriComposer(new CatalogSettings { CatalogBaseUrl = "https://cdn.test" });

        var result = composer.ComposePicUri($"{Placeholder}/images/products/1.png");

        Assert.Equal("https://cdn.test/images/products/1.png", result);
    }

    [Fact]
    public void LeavesUriWithoutPlaceholderUnchanged()
    {
        var composer = new UriComposer(new CatalogSettings { CatalogBaseUrl = "https://cdn.test" });

        var result = composer.ComposePicUri("images\\products\\1.png");

        Assert.Equal("images\\products\\1.png", result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RemovesPlaceholderGivenNoCatalogBaseUrl(string? baseUrl)
    {
        // Leaves a root-relative path, so pictures are served by the app itself.
        var composer = new UriComposer(new CatalogSettings { CatalogBaseUrl = baseUrl });

        var result = composer.ComposePicUri($"{Placeholder}/images/products/1.png");

        Assert.Equal("/images/products/1.png", result);
    }
}
