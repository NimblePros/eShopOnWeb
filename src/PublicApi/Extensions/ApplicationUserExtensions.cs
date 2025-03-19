using System;
using System.Linq;
using System.Reflection;
using Microsoft.eShopWeb.Infrastructure.Identity;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

public static class ApplicationUserExtensions
{
    public static void CopyProperties(this ApplicationUser trackedInstance, ApplicationUser updatedInstance)
    {
        // Copy all properties over except Id.
        // Id is the primary key and is used by EF Core for identifying the instance.
        if (trackedInstance == null || updatedInstance == null)
            throw new ArgumentNullException("The tracked instance and the updated values instance cannot be null");

        var properties = trackedInstance.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                  .Where(p => p.CanWrite && p.Name != "Id");

        foreach (var prop in properties)
        {
            var updatedValue = prop.GetValue(updatedInstance);
            prop.SetValue(trackedInstance, updatedValue);
        }
    }
}
