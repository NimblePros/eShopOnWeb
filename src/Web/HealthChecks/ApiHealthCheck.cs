using Microsoft.eShopWeb.Web.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Microsoft.eShopWeb.Web.HealthChecks;

public class ApiHealthCheck : IHealthCheck
{
    private readonly BaseUrlConfiguration _baseUrlConfiguration;
    private readonly IHttpClientFactory _httpClientFactory;

    public ApiHealthCheck(
        IOptions<BaseUrlConfiguration> baseUrlConfiguration,
        IHttpClientFactory httpClientFactory)
    {
        _baseUrlConfiguration = baseUrlConfiguration.Value;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_baseUrlConfiguration.ApiBase}catalog-items";

            var response = await client.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Unhealthy(
                    $"API returned {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (content.Contains(".NET Bot Black Sweatshirt"))
            {
                return HealthCheckResult.Healthy(
                    "The check indicates a healthy result.");
            }

            return HealthCheckResult.Unhealthy(
                "The check indicates an unhealthy result.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "API health check failed.", ex);
        }
    }
}
