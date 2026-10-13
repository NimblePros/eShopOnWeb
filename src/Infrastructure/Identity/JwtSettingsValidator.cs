using System.Text;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Infrastructure.Identity;

public class JwtSettingsValidator(IHostEnvironment environment) : IValidateOptions<JwtSettings>
{
    public ValidateOptionsResult Validate(string? name, JwtSettings options)
    {
        if (string.IsNullOrWhiteSpace(options.SigningKey))
        {
            return ValidateOptionsResult.Fail(
                $"A JWT signing key is required. Set '{JwtSettings.CONFIG_NAME}:SigningKey' using a secret store " +
                "(user secrets, environment variable, or Azure Key Vault).");
        }

        if (Encoding.UTF8.GetByteCount(options.SigningKey) < JwtSettings.MINIMUM_KEY_BYTES)
        {
            return ValidateOptionsResult.Fail(
                $"The JWT signing key must be at least {JwtSettings.MINIMUM_KEY_BYTES} bytes long.");
        }

        if (options.SigningKey == JwtSettings.DEVELOPMENT_SIGNING_KEY && !environment.IsLocalDevelopment())
        {
            return ValidateOptionsResult.Fail(
                $"The publicly known development JWT signing key cannot be used in the '{environment.EnvironmentName}' environment. " +
                $"Set '{JwtSettings.CONFIG_NAME}:SigningKey' to a cryptographically random value.");
        }

        if (options.TokenLifetimeMinutes <= 0)
        {
            return ValidateOptionsResult.Fail("The JWT token lifetime must be greater than zero.");
        }

        return ValidateOptionsResult.Success;
    }
}
