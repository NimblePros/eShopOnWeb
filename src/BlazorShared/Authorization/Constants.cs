namespace BlazorShared.Authorization;

public static class Constants
{
    public static class Roles
    {
        public const string ADMINISTRATORS = "Administrators";
        public const string PRODUCT_MANAGERS = "Product Managers";
    }

    // Seeded demo accounts. They use the reserved .test TLD (RFC 2606) so they never map to a real mailbox.
    public static class DemoUsers
    {
        public const string DEMO_USER = "demouser@eshop.test";
        public const string PRODUCT_MANAGER = "productmgr@eshop.test";
        public const string ADMINISTRATOR = "admin@eshop.test";
    }

    public static class RoleCombinations
    {
        public const string ADMIN_PORTAL_ROLES = $"{Roles.ADMINISTRATORS},{Roles.PRODUCT_MANAGERS}";
    }
}
