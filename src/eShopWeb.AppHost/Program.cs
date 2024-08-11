var builder = DistributedApplication.CreateBuilder(args);

var publicApi = builder.AddProject<Projects.PublicApi>(nameof(Projects.PublicApi).ToLower());

var blazorAdmin = builder
    .AddProject<Projects.BlazorAdmin>(nameof(Projects.BlazorAdmin).ToLower())
    .WithExternalHttpEndpoints()
    .WithReference(publicApi);

builder
    .AddProject<Projects.Web>(nameof(Projects.Web).ToLower())
    .WithExternalHttpEndpoints()
    .WithReference(blazorAdmin);

builder.Build().Run();
