using MyTemplate.Core.Modules.Catalog.Domain;
using MyTemplate.Core.Modules.Identity.Domain;
using MyTemplate.Core.Modules.Orders.Domain;

namespace MyTemplate.UnitTests;

public sealed class OrderPolicyTests
{
    [Fact]
    public void CanPlaceOrder_rejects_inactive_customers()
    {
        var customer = new CustomerProfile(Guid.NewGuid(), "Test Customer", CustomerStatus.Suspended);
        var product = new CatalogProduct(Guid.NewGuid(), "Test Product", 10m, IsAvailable: true);
        var requestedItem = new RequestedOrderItem(product.Id, 1);

        var decision = OrderPolicy.CanPlaceOrder(
            customer,
            [product],
            [requestedItem],
            DateTimeOffset.UtcNow);

        Assert.False(decision.Allowed);
        Assert.Equal("Customer is not allowed to place orders.", decision.Reason);
    }

    [Fact]
    public void CanPlaceOrder_prices_available_items()
    {
        var customer = new CustomerProfile(Guid.NewGuid(), "Test Customer", CustomerStatus.Active);
        var product = new CatalogProduct(Guid.NewGuid(), "Test Product", 12.50m, IsAvailable: true);
        var requestedAt = new DateTimeOffset(2026, 7, 1, 10, 0, 0, TimeSpan.Zero);

        var decision = OrderPolicy.CanPlaceOrder(
            customer,
            [product],
            [new RequestedOrderItem(product.Id, 2)],
            requestedAt);

        Assert.True(decision.Allowed);
        Assert.Equal(25m, decision.Total);
        Assert.Equal(requestedAt.AddMinutes(30), decision.EstimatedReadyAt);
    }
}
