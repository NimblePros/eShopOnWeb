using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
                 .ExcludeFromManifest()
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithEnvironment("ACCEPT_EULA", "Y");

// Random per-developer JWT signing key shared by both apps, persisted in the AppHost's user secrets
var jwtSigningKey = builder.AddParameter("jwt-signing-key",
    new GenerateParameterDefault { MinLength = 64, Special = false },
    secret: true,
    persist: true);
const string JwtSigningKeyEnvironmentVariable = "Authentication__Jwt__SigningKey";

// Forwarded to both apps so they use the in-memory databases instead of SQL Server (set in the AppHost's appsettings or environment)
const string UseOnlyInMemoryDatabaseSetting = "UseOnlyInMemoryDatabase";
var useOnlyInMemoryDatabase = builder.Configuration.GetValue<bool>(UseOnlyInMemoryDatabaseSetting);

builder
    .AddProject<Projects.PublicApi>(nameof(Projects.PublicApi).ToLower())
        .WithReference(seq)
        .WithEnvironment(JwtSigningKeyEnvironmentVariable, jwtSigningKey)
        .WithEnvironment(UseOnlyInMemoryDatabaseSetting, useOnlyInMemoryDatabase.ToString())
        .WaitFor(seq);

builder
    .AddProject<Projects.Web>(nameof(Projects.Web).ToLower())
        .WithReference(seq)
        .WithEnvironment(JwtSigningKeyEnvironmentVariable, jwtSigningKey)
        .WithEnvironment(UseOnlyInMemoryDatabaseSetting, useOnlyInMemoryDatabase.ToString())
        .WaitFor(seq);

builder.Build().Run();
