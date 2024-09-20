namespace DealBot.Application;

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var executionAssembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(config
            => config.RegisterServicesFromAssembly(executionAssembly));

        services.AddAutoMapper(executionAssembly);

        return services;
    }
}