namespace MyTemplate.Api.Endpoints;

public static class NotificationEndpoints
{
    public static IEndpointRouteBuilder MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGroup("/api/notifications")
            .WithTags("Notifications");

        return app;
    }
}
