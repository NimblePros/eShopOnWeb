var builder = DistributedApplication.CreateBuilder(args);

var publicApi = builder.AddProject<Projects.PublicApi>(nameof(Projects.PublicApi).ToLower());

builder
    .AddProject<Projects.Web>(nameof(Projects.Web).ToLower())
    .WithExternalHttpEndpoints()
    .WithReference(publicApi);

builder.Build().Run();
