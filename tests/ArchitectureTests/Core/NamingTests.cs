using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace ArchitectureTests.Core;
public class NamingTests
{
    [Fact]
    public void AggregateRootsShouldHaveAggregateInTheName()
    {
        IArchRule aggregatesShouldEndInAggregate =
            Classes()
            .That()
            .AreAssignableTo(typeof(IAggregateRoot))
            .Should()
            .HaveNameEndingWith("Aggregate");
        aggregatesShouldEndInAggregate.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void IRepositoryImplementersShouldIncludeRepositoryInName()
    {
        IArchRule repositoryImplementerNameShouldIncludeRepository =
            Classes()
            .That()
            .AreAssignableTo("Microsoft.eShopWeb.ApplicationCore.Interfaces.IRepository*", true)
            .Should()
            .HaveNameContaining("Repository");

        repositoryImplementerNameShouldIncludeRepository.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void ClassesNamedAsRepositoryShouldImplementIRepository()
    {
        IArchRule classesNamedAsRepositoryShouldImplementIRepository =
            Classes()
            .That()
            .HaveNameContaining("Repository")
            .Should()
            .ImplementInterface("Microsoft.eShopWeb.ApplicationCore.Interfaces.IRepository*", true);
        classesNamedAsRepositoryShouldImplementIRepository.Check(LayerHelper.Architecture);
    }

    [Fact]
    public void InterfacesNamedAsRepositoryShouldImplementIRepository()
    {
        IArchRule interfacesNamedAsRepositoryShouldImplementIRepository =
            Interfaces()
            .That()
            .HaveNameContaining("Repository")
            .Should()
            .ImplementInterface("Microsoft.eShopWeb.ApplicationCore.Interfaces.IRepository*", true);
        interfacesNamedAsRepositoryShouldImplementIRepository.Check(LayerHelper.Architecture);
    }


    [Fact]
    public void InterfacesWithRepositoryName_Should_Implement_IRepository()
    {
        IArchRule repositories = Interfaces()
            .That()
            .HaveNameEndingWith("Repository")
            .Should()
            .ImplementInterface(typeof(IRepository<>))
            .WithoutRequiringPositiveResults();
        repositories.Check(LayerHelper.Architecture);
    }
}
