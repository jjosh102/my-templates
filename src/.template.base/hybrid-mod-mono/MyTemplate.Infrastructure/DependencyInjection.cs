using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyTemplate.Core.Modules.Catalog.Contracts;
using MyTemplate.Core.Modules.Identity.Contracts;
using MyTemplate.Core.Modules.Notifications.Contracts;
using MyTemplate.Core.Modules.Orders.Contracts;
using MyTemplate.Infrastructure.Catalog;
using MyTemplate.Infrastructure.Configuration;
using MyTemplate.Infrastructure.Identity;
using MyTemplate.Infrastructure.Notifications;
using MyTemplate.Infrastructure.Persistence;
using MyTemplate.Infrastructure.Persistence.Stores;

namespace MyTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var persistenceOptions = PersistenceOptions.FromConfiguration(configuration);
        services.AddSingleton(persistenceOptions);

        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString(persistenceOptions.ConnectionStringName);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                options.UseInMemoryDatabase(persistenceOptions.InMemoryDatabaseName);
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        services.AddSingleton<ICustomerLookup, DemoCustomerLookup>();
        services.AddSingleton<IProductCatalog, DemoProductCatalog>();
        services.AddScoped<IOrderStore, EfOrderStore>();
        services.AddScoped<INotificationOutbox, EfNotificationOutbox>();

        return services;
    }
}
