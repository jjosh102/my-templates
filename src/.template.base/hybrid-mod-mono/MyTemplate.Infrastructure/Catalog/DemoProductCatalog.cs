using MyTemplate.Core.Modules.Catalog.Contracts;
using MyTemplate.Core.Modules.Catalog.Domain;

namespace MyTemplate.Infrastructure.Catalog;

public sealed class DemoProductCatalog : IProductCatalog
{
    public Task<IReadOnlyCollection<CatalogProduct>> FindAvailableAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<CatalogProduct> products = productIds
            .Select((id, index) => new CatalogProduct(
                id,
                $"Sample product {index + 1}",
                10m + index,
                IsAvailable: true))
            .ToArray();

        return Task.FromResult(products);
    }
}
