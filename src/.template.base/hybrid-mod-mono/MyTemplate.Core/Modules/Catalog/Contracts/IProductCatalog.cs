using MyTemplate.Core.Modules.Catalog.Domain;

namespace MyTemplate.Core.Modules.Catalog.Contracts;

public interface IProductCatalog
{
    Task<IReadOnlyCollection<CatalogProduct>> FindAvailableAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);
}
