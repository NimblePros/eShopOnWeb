using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ArchitectureTests.Core;
public class LayerDependencyTests
{

    IArchRule coreShouldNotDependOnWeb = Types()
        .That()
        .Are(LayerHelper.CoreLayer)
        .Should()
        .NotDependOnAny(LayerHelper.WebLayer)
        .Because("Core and Web should be independent of each other.");

    IArchRule coreShouldNotDependOnBlazorAdmin = Types()
           .That()
           .Are(LayerHelper.CoreLayer)
           .Should()
           .NotDependOnAny(LayerHelper.BlazorAdminLayer)
           .Because("Core and Blazor Admin should be independent of each other.");

    IArchRule coreShouldNotDependOnBlazorShared = Types()
        .That()
        .Are(LayerHelper.CoreLayer)
        .Should()
        .NotDependOnAny(LayerHelper.BlazorSharedLayer)
        .Because("Core and Blazor Shared should be independent of each other.");

    IArchRule coreShouldNotDependOnInfrastructure = Types()
            .That()
            .Are(LayerHelper.CoreLayer)
            .Should()
            .NotDependOnAny(LayerHelper.InfrastructureLayer)
            .Because("Core and Infrastructured should be independent of each other.");
    IArchRule coreShouldNotDependOnPublicAPI = Types()
            .That()
            .Are(LayerHelper.CoreLayer)
            .Should()
            .NotDependOnAny(LayerHelper.PublicAPILayer)
            .Because("Core and API should be independent of each other.");

    [Fact]
    public void CoreShouldNotDependOnBlazorAdmin()
    {
        coreShouldNotDependOnBlazorAdmin.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void CoreShouldNotDependOnBlazorShared()
    {
        coreShouldNotDependOnBlazorShared.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void CoreShouldNotDependOnInfrastructure()
    {
        coreShouldNotDependOnInfrastructure.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void CoreShouldNotDependOnPublicAPI()
    {

        coreShouldNotDependOnPublicAPI.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void CoreShouldNotDependOnWeb()
    {
        coreShouldNotDependOnWeb.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void CoreShouldNotDependOnExternalLayers()
    {
        // All these must be met
        coreShouldNotDependOnBlazorAdmin
            .And(coreShouldNotDependOnBlazorShared)
            .And(coreShouldNotDependOnInfrastructure)
            .And(coreShouldNotDependOnPublicAPI)
            .And(coreShouldNotDependOnWeb)
            .Check(LayerHelper.Architecture);
    }
}
