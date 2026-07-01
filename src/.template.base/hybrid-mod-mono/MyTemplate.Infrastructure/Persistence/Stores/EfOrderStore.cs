using MyTemplate.Core.Modules.Orders.Contracts;
using MyTemplate.Core.Modules.Orders.Domain;
using MyTemplate.Infrastructure.Persistence.Entities;

namespace MyTemplate.Infrastructure.Persistence.Stores;

public sealed class EfOrderStore(AppDbContext db) : IOrderStore
{
    public async Task SaveAsync(Order order, CancellationToken cancellationToken)
    {
        db.Orders.Add(new OrderRecord
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            Status = order.Status.ToString(),
            Total = order.Total,
            PlacedAt = order.PlacedAt,
            Items = order.Items.Select(item => new OrderItemRecord
            {
                Id = Guid.NewGuid(),
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            }).ToList()
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
