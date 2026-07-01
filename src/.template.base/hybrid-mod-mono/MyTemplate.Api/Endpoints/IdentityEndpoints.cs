namespace MyTemplate.Api.Endpoints;

public static class IdentityEndpoints
{
    public static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/identity")
            .WithTags("Identity");

        return app;
    }
}
