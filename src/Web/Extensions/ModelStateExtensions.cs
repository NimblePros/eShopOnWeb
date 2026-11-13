using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Microsoft.eShopWeb.Web.Extensions;

public static class ModelStateExtensions
{
    /// <summary>
    /// Adds each Identity error as a model-level error so it shows in the validation summary.
    /// </summary>
    public static void AddIdentityErrors(this ModelStateDictionary modelState, IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(string.Empty, error.Description);
        }
    }
}
