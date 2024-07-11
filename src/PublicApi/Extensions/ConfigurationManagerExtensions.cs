using System.IO;
using System;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.PublicApi.Extensions;

public static class ConfigurationManagerExtensions
{
    public static ConfigurationManager AddConfigurationFile(this ConfigurationManager configurationManager, string path)
    {
        var configPath = Path.Combine(AppContext.BaseDirectory, path);
        configurationManager.AddJsonFile(configPath, true, false);
        return configurationManager;
    }
}
