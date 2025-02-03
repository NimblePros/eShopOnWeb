using ArchUnitNET.xUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;


namespace ArchitectureTests.Core;
public class AccessTests
{

    [Fact]
    public void FieldsInEntitiesShouldBePrivate()
    {
        // For property members in entities
        // Make sure their setters are set to private
        FieldMembers()
            .That()
            .AreDeclaredIn(Classes().That().ResideInNamespace("Microsoft.eShopWeb.ApplicationCore.Entities*", true))
            .Should()
            .BePrivate()
            .Check(LayerHelper.Architecture);
    }
}
