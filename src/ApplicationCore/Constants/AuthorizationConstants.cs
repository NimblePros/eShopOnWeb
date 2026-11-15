namespace Microsoft.eShopWeb.ApplicationCore.Constants;

public class AuthorizationConstants
{
    public const string AUTH_KEY = "AuthKeyOfDoomThatMustBeAMinimumNumberOfBytes";

    // Only used for demo users in local development environments
    public const string DEFAULT_PASSWORD = "Pass@word1";

    // The seeded root administrator. The user management API refuses to modify or delete it.
    public const string DEFAULT_ADMIN_USERNAME = BlazorShared.Authorization.Constants.DemoUsers.ADMINISTRATOR;
}
