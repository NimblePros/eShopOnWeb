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
builder
    .AddProject<Projects.PublicApi>(nameof(Projects.PublicApi).ToLower())
        .WithReference(seq)
        .WithEnvironment(JwtSigningKeyEnvironmentVariable, jwtSigningKey)
        .WaitFor(seq);

builder
    .AddProject<Projects.Web>(nameof(Projects.Web).ToLower())
        .WithReference(seq)
        .WithEnvironment(JwtSigningKeyEnvironmentVariable, jwtSigningKey)
        .WaitFor(seq);

builder.Build().Run();
