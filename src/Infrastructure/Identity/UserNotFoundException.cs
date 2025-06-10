using System;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string identifier) : base($"No user found with identifier: {identifier}") { }
}
