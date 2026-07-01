namespace MyTemplate.Core.Modules.Catalog.Domain;

public sealed record CatalogProduct(
    Guid Id,
    string Name,
    decimal Price,
    bool IsAvailable);
