using System.Text.Json;
using NServiceBus;

var builder = Host.CreateDefaultBuilder();

builder.UseConsoleLifetime();
builder.UseNServiceBus(context =>
{
    var endpointConfiguration = new EndpointConfiguration("orders-worker");
    endpointConfiguration.UseSerialization<SystemJsonSerializer>();
    endpointConfiguration.UseTransport<LearningTransport>();
    return endpointConfiguration;
});

var host = builder.Build();
host.Run();
