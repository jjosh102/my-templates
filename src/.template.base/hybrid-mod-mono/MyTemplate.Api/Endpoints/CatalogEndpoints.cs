namespace MyTemplate.Api.Endpoints;

public static class CatalogEndpoints
{
    public static IEndpointRouteBuilder MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/catalog")
            .WithTags("Catalog");

        return app;
    }
}
