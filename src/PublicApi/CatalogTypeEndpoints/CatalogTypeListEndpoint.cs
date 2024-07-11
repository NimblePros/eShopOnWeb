using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FastEndpoints;
using Microsoft.AspNetCore.Http;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.PublicApi.CatalogTypeEndpoints;

/// <summary>
/// List Catalog Types
/// </summary>
public class CatalogTypeListEndpoint : EndpointWithoutRequest<ListCatalogTypesResponse>
{
    private readonly IRepository<CatalogType> _catalogTypeRepository;
    private readonly AutoMapper.IMapper _mapper;

    public CatalogTypeListEndpoint(IRepository<CatalogType> catalogTypeRepository, AutoMapper.IMapper mapper)
    {
        _catalogTypeRepository = catalogTypeRepository;
        _mapper = mapper;

    }

    public override void Configure()
    {
        Get("api/catalog-types");
        AllowAnonymous();
        Description(d =>
            d.Produces<ListCatalogTypesResponse>()
             .WithTags("CatalogTypeEndpoints"));
    }

    public override async Task<ListCatalogTypesResponse> ExecuteAsync(CancellationToken ct)
    {
        var response = new ListCatalogTypesResponse();

        var items = await _catalogTypeRepository.ListAsync(ct);

        response.CatalogTypes.AddRange(items.Select(_mapper.Map<CatalogTypeDto>));

        return response;
    }
}
