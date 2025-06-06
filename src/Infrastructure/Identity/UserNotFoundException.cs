using System;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message) : base(message)
    {
    }
}
