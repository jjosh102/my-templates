using MyTemplate.Core.Modules.Identity.Domain;

namespace MyTemplate.Core.Modules.Identity.Contracts;

public interface ICustomerLookup
{
    Task<CustomerProfile?> FindAsync(Guid customerId, CancellationToken cancellationToken);
}
