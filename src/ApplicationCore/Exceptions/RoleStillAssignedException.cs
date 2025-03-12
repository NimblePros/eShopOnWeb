using System;

namespace Microsoft.eShopWeb.ApplicationCore.Exceptions;
public class RoleStillAssignedException : Exception
{
    public RoleStillAssignedException(string message) : base(message)
    {

    }
}
