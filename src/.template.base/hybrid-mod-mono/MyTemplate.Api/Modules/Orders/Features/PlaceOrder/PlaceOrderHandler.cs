using Microsoft.EntityFrameworkCore;
using MyTemplate.Api.Infrastructure.Persistence;
using MyTemplate.Api.Modules.Orders.Domain;
using MyTemplate.Api.Shared.Results;
using MyTemplate.Api.Shared.Time;

namespace MyTemplate.Api.Modules.Orders.Features.PlaceOrder;

public sealed class PlaceOrderHandler
{
    private readonly AppDbContext _db;
    private readonly IClock _clock;

    public PlaceOrderHandler(AppDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<Result<PlaceOrderResponse>> HandleAsync(
        PlaceOrderRequest request,
        CancellationToken cancellationToken)
    {
        // Example: Validate customer exists (mocked for simplicity here, or query actual DB)
        if (request.CustomerId == Guid.Empty)
        {
            return Result<PlaceOrderResponse>.Validation("Customer was not found.");
        }

        var productIds = request.Items.Select(x => x.ProductId).Distinct().ToList();

        // In a real app we would query Catalog module products to ensure they exist and get prices.
        // For the template, we mock product price.
        var items = request.Items.Select(x => new OrderItem(x.ProductId, x.Quantity, 10.0m)).ToList();

        var order = Order.Place(
            request.CustomerId,
            items,
            _clock.UtcNow);

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<PlaceOrderResponse>.Created(new PlaceOrderResponse(
            order.Id,
            order.Status.ToString(),
            order.Total));
    }
}
