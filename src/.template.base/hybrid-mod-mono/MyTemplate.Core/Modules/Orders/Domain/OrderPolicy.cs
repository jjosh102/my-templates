using MyTemplate.Core.Modules.Catalog.Domain;
using MyTemplate.Core.Modules.Identity.Domain;

namespace MyTemplate.Core.Modules.Orders.Domain;

public static class OrderPolicy
{
    public static OrderPlacementDecision CanPlaceOrder(
        CustomerProfile customer,
        IReadOnlyCollection<CatalogProduct> products,
        IReadOnlyCollection<RequestedOrderItem> requestedItems,
        DateTimeOffset requestedAt)
    {
        if (!customer.CanPlaceOrders)
            return OrderPlacementDecision.Reject("Customer is not allowed to place orders.");

        if (requestedItems.Count == 0)
            return OrderPlacementDecision.Reject("An order requires at least one item.");

        var availableProducts = products
            .Where(x => x.IsAvailable)
            .ToDictionary(x => x.Id);

        var orderItems = new List<OrderItem>();
        foreach (var item in requestedItems)
        {
            if (item.Quantity <= 0)
                return OrderPlacementDecision.Reject("Order item quantity must be greater than zero.");

            if (!availableProducts.TryGetValue(item.ProductId, out var product))
                return OrderPlacementDecision.Reject($"Product '{item.ProductId}' is not available.");

            orderItems.Add(new OrderItem(product.Id, product.Name, item.Quantity, product.Price));
        }

        return OrderPlacementDecision.Accept(orderItems, requestedAt.AddMinutes(30));
    }
}
