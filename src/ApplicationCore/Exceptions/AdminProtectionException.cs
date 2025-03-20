using System;

namespace Microsoft.eShopWeb.ApplicationCore.Exceptions;
public class AdminProtectionException : Exception
{
    public AdminProtectionException() : base($"The admin account cannot be deleted at this time.")
    {
    }
}
