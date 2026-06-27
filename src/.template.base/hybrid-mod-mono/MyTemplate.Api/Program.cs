using MyTemplate.Api.Infrastructure;
using MyTemplate.Api.Modules.Catalog;
using MyTemplate.Api.Modules.Identity;
using MyTemplate.Api.Modules.Notifications;
using MyTemplate.Api.Modules.Orders;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddOpenApi();

builder.Services.AddInfrastructure(builder.Configuration);

// Register Modules
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCatalogModule();
builder.Services.AddOrdersModule();
builder.Services.AddNotificationsModule();

var MyTemplate = builder.Build();

MyTemplate.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (MyTemplate.Environment.IsDevelopment())
{
    MyTemplate.MapOpenApi();
}

MyTemplate.UseHttpsRedirection();

// Map Module Endpoints
MyTemplate.MapIdentityModule();
MyTemplate.MapCatalogModule();
MyTemplate.MapOrdersModule();
MyTemplate.MapNotificationsModule();

MyTemplate.Run();
