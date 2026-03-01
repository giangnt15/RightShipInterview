using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RightShip.Core.Application.Uow;
using RightShip.OrderService.Domain.Repositories;

namespace RightShip.OrderService.Persistence.EfCore;

/// <summary>
/// DI registration for Order persistence (EF Core, repository, and unit of work).
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Order DbContext factory, OrderUnitOfWork (EfCoreUow), and IUnitOfWork to the service collection.
    /// IOrderRepository is obtained via IUnitOfWork.GetRepository&lt;IOrderRepository&gt;().
    /// </summary>
    public static IServiceCollection AddOrderPersistence(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
    {
        services.AddPooledDbContextFactory<OrderDbContext>(configureOptions);
        services.AddScoped<OrderDbContextFactory>();
        services.AddScoped<IUnitOfWork, OrderUnitOfWork>();
        return services;
    }
}
