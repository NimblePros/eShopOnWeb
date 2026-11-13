using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.PublicApi.CatalogBrandEndpoints;
using Microsoft.eShopWeb.PublicApi.CatalogItemEndpoints;
using Microsoft.eShopWeb.PublicApi.CatalogTypeEndpoints;

namespace Microsoft.eShopWeb.PublicApi;

public static class CatalogMappingExtensions
{
    public static CatalogItemDto ToDto(this CatalogItem item) => new()
    {
        Id = item.Id,
        Name = item.Name,
        Description = item.Description,
        Price = item.Price,
        PictureUri = item.PictureUri,
        CatalogTypeId = item.CatalogTypeId,
        CatalogBrandId = item.CatalogBrandId
    };

    public static CatalogTypeDto ToDto(this CatalogType type) => new()
    {
        Id = type.Id,
        Name = type.Type
    };

    public static CatalogBrandDto ToDto(this CatalogBrand brand) => new()
    {
        Id = brand.Id,
        Name = brand.Brand
    };
}
