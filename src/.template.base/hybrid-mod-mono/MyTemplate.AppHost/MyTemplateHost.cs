var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
                 .WithDataVolume()
                 .AddDatabase("database");

builder.AddProject<Projects.MyTemplate_Api>("api")
       .WithReference(sql);

builder.Build().Run();
