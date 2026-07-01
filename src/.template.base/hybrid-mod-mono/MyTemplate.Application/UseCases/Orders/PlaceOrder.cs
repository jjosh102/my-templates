using MyTemplate.Application.Common.Results;
using MyTemplate.Core.Modules.Catalog.Contracts;
using MyTemplate.Core.Modules.Identity.Contracts;
using MyTemplate.Core.Modules.Notifications.Contracts;
using MyTemplate.Core.Modules.Notifications.Domain;
using MyTemplate.Core.Modules.Orders.Contracts;
using MyTemplate.Core.Modules.Orders.Domain;

namespace MyTemplate.Application.UseCases.Orders;

public sealed record PlaceOrderRequest(
    Guid CustomerId,
    IReadOnlyList<PlaceOrderItemRequest> Items);

public sealed record PlaceOrderItemRequest(
    Guid ProductId,
    int Quantity);

public sealed record PlaceOrderResponse(
    Guid OrderId,
    string Status,
    decimal Total,
    DateTimeOffset EstimatedReadyAt);

public sealed class PlaceOrder(
    ICustomerLookup customers,
    IProductCatalog catalog,
    IOrderStore orders,
    INotificationOutbox notifications,
    TimeProvider clock)
{
    public async Task<Result<PlaceOrderResponse>> ExecuteAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        var customer = await customers.FindAsync(request.CustomerId, cancellationToken);
        if (customer is null)
        {
            return Result<PlaceOrderResponse>.NotFound(
                "Customer.NotFound",
                "Customer was not found.");
        }

        var requestedItems = request.Items
            .Select(x => new RequestedOrderItem(x.ProductId, x.Quantity))
            .ToArray();
        var productIds = requestedItems
            .Select(x => x.ProductId)
            .Distinct()
            .ToArray();
        var products = await catalog.FindAvailableAsync(productIds, cancellationToken);
        var requestedAt = clock.GetUtcNow();

        var decision = OrderPolicy.CanPlaceOrder(customer, products, requestedItems, requestedAt);
        if (!decision.Allowed)
        {
            return Result<PlaceOrderResponse>.Conflict(
                "Order.NotAllowed",
                decision.Reason ?? "The order cannot be placed.");
        }

        var order = Order.Place(customer.Id, decision.Items, requestedAt);
        await orders.SaveAsync(order, cancellationToken);

        await notifications.EnqueueAsync(
            new NotificationMessage(
                customer.Id,
                "Order confirmation",
                $"Order {order.Id} was placed successfully.",
                requestedAt),
            cancellationToken);

        return Result<PlaceOrderResponse>.Created(
            new PlaceOrderResponse(
                order.Id,
                order.Status.ToString(),
                order.Total,
                decision.EstimatedReadyAt));
    }
}
