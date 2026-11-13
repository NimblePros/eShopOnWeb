using Microsoft.eShopWeb.ApplicationCore.Entities;
using Xunit;

namespace Microsoft.eShopWeb.UnitTests.ApplicationCore.Entities.CatalogItemTests;

public class UpdatePictureUri
{
    private static CatalogItem CreateCatalogItem() =>
        new(catalogTypeId: 1, catalogBrandId: 1, "Description", "Name", 1.00m, "images\\products\\original.png");

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SetsPictureUriToEmptyGivenNullOrEmptyPictureName(string? pictureName)
    {
        var catalogItem = CreateCatalogItem();

        catalogItem.UpdatePictureUri(pictureName!);

        Assert.Equal(string.Empty, catalogItem.PictureUri);
    }

    [Fact]
    public void SetsPictureUriToProductImagePathGivenPictureName()
    {
        var catalogItem = CreateCatalogItem();

        catalogItem.UpdatePictureUri("new.png");

        Assert.StartsWith("images\\products\\new.png?", catalogItem.PictureUri);
    }

    [Fact]
    public void AppendsCurrentUtcTicksAsCacheBuster()
    {
        var catalogItem = CreateCatalogItem();
        var ticksBefore = DateTime.UtcNow.Ticks;

        catalogItem.UpdatePictureUri("new.png");

        var ticksAfter = DateTime.UtcNow.Ticks;
        var cacheBuster = long.Parse(catalogItem.PictureUri.Split('?')[1]);
        Assert.InRange(cacheBuster, ticksBefore, ticksAfter);
    }
}
