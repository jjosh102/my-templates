namespace MyTemplate.Infrastructure.Persistence.Entities;

public sealed class OrderRecord
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public DateTimeOffset PlacedAt { get; set; }
    public List<OrderItemRecord> Items { get; set; } = [];
}
