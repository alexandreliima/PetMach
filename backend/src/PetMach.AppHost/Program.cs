IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

IResourceBuilder<PostgresServerResource> postgres = builder
    .AddPostgres("postgres")
    .WithDataVolume("petmach-postgres-data");

IResourceBuilder<PostgresDatabaseResource> database = postgres.AddDatabase("petmach");

IResourceBuilder<ProjectResource> api = builder
    .AddProject<Projects.PetMach_Api>("api")
    .WithReference(database)
    .WaitFor(database)
    .WithExternalHttpEndpoints();

builder
    .AddProject<Projects.PetMach_Admin>("admin")
    .WithReference(api)
    .WaitFor(api)
    .WithExternalHttpEndpoints();

builder.Build().Run();
