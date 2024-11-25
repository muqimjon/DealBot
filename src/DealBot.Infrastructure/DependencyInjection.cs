namespace DealBot.Infrastructure;

using DealBot.Application.Common;
using DealBot.Infrastructure.Persistence.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<IAppDbContext, AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString(name: "DefaultConnection")));

        return services;
    }
}