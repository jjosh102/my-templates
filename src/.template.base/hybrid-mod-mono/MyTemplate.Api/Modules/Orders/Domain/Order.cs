namespace MyTemplate.Api.Modules.Orders.Domain;

public sealed class Order
{
    public Guid Id { get; private set; }
    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal Total { get; private set; }
    public DateTimeOffset PlacedAt { get; private set; }

    private readonly List<OrderItem> _items = new();
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { } // EF Core

    public static Order Place(Guid customerId, IEnumerable<OrderItem> items, DateTimeOffset placedAt)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            Status = OrderStatus.Pending,
            PlacedAt = placedAt
        };

        order._items.AddRange(items);
        order.Total = order._items.Sum(x => x.Price * x.Quantity);

        return order;
    }
}

public enum OrderStatus
{
    Pending,
    Shipped,
    Cancelled
}
