using Microsoft.AspNetCore.Mvc;
using MyTemplate.Api.Http;
using MyTemplate.Application.UseCases.Orders;

namespace MyTemplate.Api.Endpoints;

public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/orders")
            .WithTags("Orders");

        group.MapPost("/", PlaceOrderAsync)
            .WithName("PlaceOrder")
            .AddEndpointFilter<ValidationFilter<PlaceOrderRequest>>()
            .Produces<PlaceOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }

    private static async Task<IResult> PlaceOrderAsync(
        [FromBody] PlaceOrderRequest request,
        [FromServices] PlaceOrder placeOrder,
        CancellationToken cancellationToken)
    {
        var result = await placeOrder.ExecuteAsync(request, cancellationToken);
        return result.ToHttpResult();
    }
}
