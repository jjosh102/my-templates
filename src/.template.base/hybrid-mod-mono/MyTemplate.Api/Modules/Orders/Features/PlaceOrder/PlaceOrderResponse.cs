namespace MyTemplate.Api.Modules.Orders.Features.PlaceOrder;

public sealed record PlaceOrderResponse(
    Guid OrderId,
    string Status,
    decimal Total);
