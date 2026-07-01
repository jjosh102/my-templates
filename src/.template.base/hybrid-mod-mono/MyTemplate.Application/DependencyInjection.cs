using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MyTemplate.Application.UseCases.Orders;

namespace MyTemplate.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<PlaceOrder>();
        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<PlaceOrderValidator>();

        return services;
    }
}
