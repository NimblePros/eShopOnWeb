using Xunit;

namespace Microsoft.eShopWeb.FunctionalTests.Web;

/// <summary>
/// Every Web functional test class shares one <see cref="TestApplication"/> host. Starting a host
/// (building the app and seeding the in-memory databases) is the slowest part of these tests, and
/// the databases are shared by name across hosts anyway, so one host per class bought no isolation.
/// Tests that change account state use throwaway users (<see cref="WebPageHelpers.CreateUserAsync"/>).
/// The collection also runs its classes one at a time, as they sign in as the same demo user.
/// </summary>
[CollectionDefinition(Name)]
public sealed class WebTestCollection : ICollectionFixture<TestApplication>
{
    public const string Name = "Web";
}
