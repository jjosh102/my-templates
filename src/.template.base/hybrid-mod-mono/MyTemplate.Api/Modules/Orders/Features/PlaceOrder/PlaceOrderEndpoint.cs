using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyTemplate.Api.Shared.Results;
using MyTemplate.Api.Shared.Validation;

namespace MyTemplate.Api.Modules.Orders.Features.PlaceOrder;

public static class PlaceOrderEndpoint
{
    public static RouteGroupBuilder MapPlaceOrderEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .WithName("PlaceOrder")
            .AddEndpointFilter<ValidationFilter<PlaceOrderRequest>>()
            .Produces<PlaceOrderResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        [FromBody] PlaceOrderRequest request,
        [FromServices] PlaceOrderHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(request, cancellationToken);
        return result.ToHttpResult();
    }
}
