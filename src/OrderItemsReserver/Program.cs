using Azure.Storage.Blobs;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OrderItemsReserver.Models;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        var conn = Environment.GetEnvironmentVariable("OrdersBlobConnection");
        services.AddSingleton(new BlobServiceClient(conn));
        services.Configure<OrdersOptions>(opts =>
        {
            var name = Environment.GetEnvironmentVariable("OrdersContainerName");
            if (!string.IsNullOrWhiteSpace(name)) opts.ContainerName = name!;
        });
    })
    .Build();

await host.RunAsync();
