using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using MyTemplate.Api.Modules.Orders.Features.PlaceOrder;

namespace MyTemplate.Api.Modules.Orders;

public static class OrdersModule
{
    public static IServiceCollection AddOrdersModule(this IServiceCollection services)
    {
        services.AddScoped<PlaceOrderHandler>();
        
        // Register validators from the current assembly
        services.AddValidatorsFromAssemblyContaining<PlaceOrderValidator>();

        return services;
    }

    public static IEndpointRouteBuilder MapOrdersModule(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapPlaceOrderEndpoint();

        return app;
    }
}
