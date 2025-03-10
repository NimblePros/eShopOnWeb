var builder = DistributedApplication.CreateBuilder(args);

var seq = builder.AddSeq("seq")
                 .ExcludeFromManifest()
                 .WithLifetime(ContainerLifetime.Persistent)
                 .WithEnvironment("ACCEPT_EULA", "Y");
builder
    .AddProject<Projects.PublicApi>(nameof(Projects.PublicApi).ToLower())
        .WithReference(seq)
        .WaitFor(seq);

builder
    .AddProject<Projects.Web>(nameof(Projects.Web).ToLower())
        .WithReference(seq)
        .WaitFor(seq);

builder.Build().Run();
