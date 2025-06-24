using System.Text.Json;
using Microsoft.eShopWeb.ApplicationCore.Configuration;
using NServiceBus;

var builder = Host.CreateDefaultBuilder();

builder.UseConsoleLifetime();
builder.UseNServiceBus(context => NServiceBusConfiguration.GetNServiceBusConfiguration());

var host = builder.Build();
host.Run();
