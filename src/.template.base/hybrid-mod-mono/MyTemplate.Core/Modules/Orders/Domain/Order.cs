namespace MyTemplate.Core.Modules.Orders.Domain;

public sealed class Order
{
    private readonly List<OrderItem> _items;

    private Order(
        Guid id,
        Guid customerId,
        OrderStatus status,
        decimal total,
        DateTimeOffset placedAt,
        IReadOnlyCollection<OrderItem> items)
    {
        Id = id;
        CustomerId = customerId;
        Status = status;
        Total = total;
        PlacedAt = placedAt;
        _items = items.ToList();
    }

    public Guid Id { get; }
    public Guid CustomerId { get; }
    public OrderStatus Status { get; }
    public decimal Total { get; }
    public DateTimeOffset PlacedAt { get; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    public static Order Place(
        Guid customerId,
        IReadOnlyCollection<OrderItem> items,
        DateTimeOffset placedAt)
    {
        if (customerId == Guid.Empty)
            throw new ArgumentException("Customer id is required.", nameof(customerId));

        if (items.Count == 0)
            throw new ArgumentException("An order requires at least one item.", nameof(items));

        return new Order(
            Guid.NewGuid(),
            customerId,
            OrderStatus.Pending,
            items.Sum(x => x.LineTotal),
            placedAt,
            items);
    }
}
