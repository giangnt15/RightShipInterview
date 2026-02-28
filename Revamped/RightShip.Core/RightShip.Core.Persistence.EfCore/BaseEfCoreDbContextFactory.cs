using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace RightShip.Core.Persistence.EfCore;

/// <summary>
/// Base class for DbContextFactory.
/// Can be used to create a DbContext instance and set necessary properties if needed.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext, must implement <see cref="BaseEfCoreDbContext"/></typeparam>
public class BaseEfCoreDbContextFactory<TDbContext> : IDbContextFactory<TDbContext> where TDbContext : BaseEfCoreDbContext
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly IDbContextFactory<TDbContext> DbContextFactory;

    public BaseEfCoreDbContextFactory(IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        DbContextFactory = serviceProvider.GetRequiredService<IDbContextFactory<TDbContext>>();
    }

    /// <summary>
    /// Create DbContext in a synchronous manner.
    /// Use <see cref="CreateDbContextAsync(CancellationToken)"/> if possible
    /// </summary>
    /// <returns>The DbContext instance</returns>
    public virtual TDbContext CreateDbContext()
    {
        var dbContext = DbContextFactory.CreateDbContext();
        return dbContext;
    }

    /// <summary>
    /// Create DbContext in a asynchronous manner.
    /// </summary>
    /// <returns>The DbContext instance</returns>
    public virtual async Task<TDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
    {
        var dbContext = await DbContextFactory.CreateDbContextAsync(cancellationToken);
        return dbContext;
    }
}