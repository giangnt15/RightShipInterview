using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RightShip.Core.Persistence.EfCore;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add DbContext to DI with a custom DbContextOptionsBuilder.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="builder">The DbContextOptionsBuilder.</param>
    /// <typeparam name="T">The type of the DbContext, must implement <see cref="BaseEfCoreDbContext"/></typeparam>
    /// <typeparam name="TFactory">The type of the DbContextFactory, must implement <see cref="BaseEfCoreDbContextFactory"/></typeparam>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddCoreDbContext<T, TFactory>(this IServiceCollection services, Action<DbContextOptionsBuilder> builder)
     where T : BaseEfCoreDbContext
     where TFactory : BaseEfCoreDbContextFactory<T>
    {
        services.AddScoped<TFactory>();
        services.AddPooledDbContextFactory<T>((sp, options) =>
        {
            builder.Invoke(options);
        });
        return services;
    }
}