namespace MyTemplate.Core.Modules.Orders.Domain;

public sealed record RequestedOrderItem(Guid ProductId, int Quantity);
