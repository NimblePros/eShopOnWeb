using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;

var host = new HostBuilder()
    .ConfigureAppConfiguration((ctx, cfg) =>
    {
        cfg.AddEnvironmentVariables(); // important for Azure App Settings
    })
    .ConfigureFunctionsWebApplication() // Use ASP.NET Core integration for Functions
    .ConfigureServices((ctx, services) =>
    {
        services.AddSingleton(sp =>
        {
            var cfg = sp.GetRequiredService<IConfiguration>();

            var conn = cfg["CosmosConnection"] // preferred for Functions (App Settings / Values)
                ?? cfg.GetConnectionString("CosmosConnection")
                ?? cfg["ConnectionStrings:CosmosConnection"];

            if (string.IsNullOrWhiteSpace(conn))
            {
                throw new InvalidOperationException(
                    "Missing Cosmos DB connection string. Set 'CosmosConnection' in Application settings (or in local.settings.json under Values)."
                );
            }

            var client = new CosmosClient(conn, new CosmosClientOptions
            {
                ApplicationName = "DeliveryOrderProcessor",
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });

            // Ensure database and container exist with PK = /id
            var db = client.CreateDatabaseIfNotExistsAsync("deliverydb").GetAwaiter().GetResult();
            db.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties("orders", "/id"), throughput: 400
            ).GetAwaiter().GetResult();

            return client;
        });
    })
    .Build();

host.Run();
