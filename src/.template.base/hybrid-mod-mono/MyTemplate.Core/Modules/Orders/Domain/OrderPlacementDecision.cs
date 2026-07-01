namespace MyTemplate.Core.Modules.Orders.Domain;

public sealed record OrderPlacementDecision(
    bool Allowed,
    string? Reason,
    IReadOnlyCollection<OrderItem> Items,
    decimal Total,
    DateTimeOffset EstimatedReadyAt)
{
    public static OrderPlacementDecision Accept(
        IReadOnlyCollection<OrderItem> items,
        DateTimeOffset estimatedReadyAt)
    {
        return new OrderPlacementDecision(
            true,
            null,
            items,
            items.Sum(x => x.LineTotal),
            estimatedReadyAt);
    }

    public static OrderPlacementDecision Reject(string reason)
    {
        return new OrderPlacementDecision(
            false,
            reason,
            Array.Empty<OrderItem>(),
            0m,
            DateTimeOffset.MinValue);
    }
}
