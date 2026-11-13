using System;
using System.Linq;
using Microsoft.Extensions.DependencyModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace PublicApiIntegrationTests;

[TestClass]
public class DependencyTests
{
    // BlazorInputFile 0.2.0 has a type that can't be loaded on current runtimes. When it was on the
    // PublicApi dependency graph, FastEndpoints' assembly scan threw and the host never started (#281).
    // Program.cs now limits that scan to the PublicApi assembly, and the endpoint tests cover host
    // startup. This test keeps the obsolete package from coming back (it was replaced by the
    // built-in Microsoft.AspNetCore.Components.Forms.InputFile).
    [TestMethod]
    public void PublicApiDoesNotDependOnBlazorInputFile()
    {
        var dependencyContext = DependencyContext.Default;
        Assert.IsNotNull(dependencyContext);

        var libraryNames = dependencyContext.RuntimeLibraries
            .Select(library => library.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Guards against a vacuous pass if the dependency context isn't the one we expect.
        Assert.IsTrue(libraryNames.Contains("PublicApi"), "PublicApi should be in the dependency context.");
        Assert.IsFalse(libraryNames.Contains("BlazorInputFile"), "BlazorInputFile should not be on the PublicApi dependency graph.");
    }
}
