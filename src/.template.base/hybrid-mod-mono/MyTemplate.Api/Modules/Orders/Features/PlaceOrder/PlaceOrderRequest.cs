namespace MyTemplate.Api.Modules.Orders.Features.PlaceOrder;

public sealed record PlaceOrderRequest(
    Guid CustomerId,
    IReadOnlyList<PlaceOrderItemRequest> Items);

public sealed record PlaceOrderItemRequest(
    Guid ProductId,
    int Quantity);
