using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RightShip.Core.Application.Uow;
using RightShip.ProductService.Domain.Repositories;

namespace RightShip.ProductService.Persistence.EfCore;

/// <summary>
/// DI registration for Product persistence (EF Core, repository, and unit of work).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Product DbContext factory, ProductUnitOfWork (EfCoreUow), and IUnitOfWork to the service collection.
    /// IProductRepository is obtained via IUnitOfWork.GetRepository&lt;IProductRepository&gt;().
    /// </summary>
    public static IServiceCollection AddProductPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        services.AddPooledDbContextFactory<ProductDbContext>(configureOptions);
        services.AddScoped<ProductDbContextFactory>();
        services.AddScoped<IUnitOfWork, ProductUnitOfWork>();
        return services;
    }
}
