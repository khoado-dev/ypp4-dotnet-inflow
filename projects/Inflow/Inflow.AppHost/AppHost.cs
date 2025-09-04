var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Inflow>("inflow");

builder.Build().Run();