using MyTemplate.Core.Modules.Identity.Contracts;
using MyTemplate.Core.Modules.Identity.Domain;

namespace MyTemplate.Infrastructure.Identity;

public sealed class DemoCustomerLookup : ICustomerLookup
{
    public Task<CustomerProfile?> FindAsync(Guid customerId, CancellationToken cancellationToken)
    {
        if (customerId == Guid.Empty)
            return Task.FromResult<CustomerProfile?>(null);

        var customer = new CustomerProfile(customerId, "Demo Customer", CustomerStatus.Active);
        return Task.FromResult<CustomerProfile?>(customer);
    }
}
