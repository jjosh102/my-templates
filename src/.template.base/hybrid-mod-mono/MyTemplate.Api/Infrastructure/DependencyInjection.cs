using Microsoft.EntityFrameworkCore;
using MyTemplate.Api.Infrastructure.Persistence;
using MyTemplate.Api.Shared.Time;

namespace MyTemplate.Api.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IClock, SystemClock>();
        
        services.AddDbContext<AppDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("database");
            if (string.IsNullOrEmpty(connectionString))
            {
                options.UseInMemoryDatabase("MyTemplateDb");
            }
            else
            {
                options.UseSqlServer(connectionString);
            }
        });

        return services;
    }
}
