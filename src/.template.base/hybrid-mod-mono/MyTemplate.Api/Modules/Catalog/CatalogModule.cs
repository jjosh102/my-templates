using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace MyTemplate.Api.Modules.Catalog;

public static class CatalogModule
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        // Register Catalog specific services here
        return services;
    }

    public static IEndpointRouteBuilder MapCatalogModule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/catalog")
            .WithTags("Catalog");

        // Map Catalog endpoints here

        return app;
    }
}
