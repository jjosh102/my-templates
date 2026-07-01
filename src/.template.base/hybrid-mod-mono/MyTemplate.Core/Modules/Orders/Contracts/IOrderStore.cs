using MyTemplate.Core.Modules.Orders.Domain;

namespace MyTemplate.Core.Modules.Orders.Contracts;

public interface IOrderStore
{
    Task SaveAsync(Order order, CancellationToken cancellationToken);
}
