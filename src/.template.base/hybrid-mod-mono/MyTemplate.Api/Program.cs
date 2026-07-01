using MyTemplate.Api.Endpoints;
using MyTemplate.Application;
using MyTemplate.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var MyTemplate = builder.Build();

MyTemplate.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (MyTemplate.Environment.IsDevelopment())
{
    MyTemplate.MapOpenApi();
}

MyTemplate.UseHttpsRedirection();

MyTemplate.MapCatalogEndpoints();
MyTemplate.MapIdentityEndpoints();
MyTemplate.MapOrdersEndpoints();
MyTemplate.MapNotificationEndpoints();

MyTemplate.Run();
