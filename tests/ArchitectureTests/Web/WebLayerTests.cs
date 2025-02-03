namespace ArchitectureTests.Web;

using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;


public class WebLayerTests
{
    // This should fail.
    [Fact]
    public void WebShouldNotDependOnBlazorAdmin()
    {
        IArchRule webShouldNotDependOnBlazorAdmin = Types()
            .That()
            .Are(LayerHelper.WebLayer)
            .Should()
            .NotDependOnAny(LayerHelper.BlazorAdminLayer)
            .Because("BlazorAdmin and Web should be independent of each other.");
        webShouldNotDependOnBlazorAdmin.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void WebShouldNotDependOnAPI()
    {
        IArchRule webShouldNotDependOnAPI = Types()
            .That()
            .Are(LayerHelper.WebLayer)
            .Should()
            .NotDependOnAny(LayerHelper.PublicAPILayer)
            .Because("Web and API should be independent of each other.");
        webShouldNotDependOnAPI.Check(LayerHelper.Architecture);
    }
}
