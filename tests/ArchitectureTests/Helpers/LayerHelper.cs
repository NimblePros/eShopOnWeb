using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using BlazorAdmin.Services;
using BlazorShared.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.Infrastructure.Data;
using Microsoft.eShopWeb.PublicApi;
using Microsoft.eShopWeb.Web.Features.OrderDetails;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

internal class LayerHelper
{
    private static System.Reflection.Assembly coreAssembly = typeof(IOrderService).Assembly;
    private static System.Reflection.Assembly blazorAdminAssembly = typeof(HttpService).Assembly;
    private static System.Reflection.Assembly blazorSharedAssembly = typeof(ICatalogItemService).Assembly;
    private static System.Reflection.Assembly infrastructureAssembly = typeof(FileItem).Assembly;
    private static System.Reflection.Assembly publicAPIAssembly = typeof(BaseMessage).Assembly;
    private static System.Reflection.Assembly webAssembly = typeof(GetOrderDetails).Assembly;

    internal static readonly Architecture Architecture = new ArchLoader()
        .LoadAssemblies(
            coreAssembly,
            blazorAdminAssembly,
            blazorSharedAssembly,
            infrastructureAssembly,
            publicAPIAssembly,
            webAssembly
        )
        .Build();

    internal static readonly IObjectProvider<IType> CoreLayer = Types()
        .That()
        .ResideInAssembly(coreAssembly)
        .As("Core Layer");

    internal static readonly IObjectProvider<IType> BlazorAdminLayer = Types()
        .That()
        .ResideInAssembly(blazorAdminAssembly)
        .As("BlazorAdmin Layer");

    internal static readonly IObjectProvider<IType> BlazorSharedLayer = Types()
        .That()
        .ResideInAssembly(blazorSharedAssembly)
        .As("BlazorShared Layer");

    internal static readonly IObjectProvider<IType> InfrastructureLayer = Types()
        .That()
        .ResideInAssembly(infrastructureAssembly)
        .As("Infrastructure Layer");

    internal static readonly IObjectProvider<IType> PublicAPILayer = Types()
        .That()
        .ResideInAssembly(publicAPIAssembly)
        .As("API Layer");

    internal static readonly IObjectProvider<IType> WebLayer = Types()
        .That()
        .ResideInAssembly(webAssembly)
        .As("Web Layer");
}
