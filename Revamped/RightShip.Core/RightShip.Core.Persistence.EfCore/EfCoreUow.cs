using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RightShip.Core.Application.Uow;
using RightShip.Core.Domain.Repositories;

namespace RightShip.Core.Persistence.EfCore;

/// <summary>
/// The implementation of the unit of work pattern for EF Core.
/// </summary>
/// <typeparam name="TDbContext">The type of the DbContext, must implement <see cref="BaseEfCoreDbContext"/></typeparam>
/// <typeparam name="TDbContextFactory">The type of the DbContextFactory, must implement <see cref="BaseEfCoreDbContextFactory"/></typeparam>
public class EfCoreUow<TDbContext, TDbContextFactory> : IUnitOfWork where TDbContext : BaseEfCoreDbContext where TDbContextFactory : BaseEfCoreDbContextFactory<TDbContext>, IDisposable, IAsyncDisposable
{
    protected readonly TDbContextFactory _dbContextFactory;
    protected TDbContext? _dbContext;
    private readonly IServiceProvider _serviceProvider;

    public EfCoreUow(TDbContextFactory dbContextFactory, IServiceProvider serviceProvider)
    {
        _dbContextFactory = dbContextFactory;
        _serviceProvider = serviceProvider;
    }

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_dbContext == null)
        {
            throw new InvalidOperationException("Unit of work not started");
        }
        return await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        // No need to rollback, EF Core will handle it automatically
        return Task.CompletedTask;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _dbContext ??= await _dbContextFactory.CreateDbContextAsync(cancellationToken);
    }

    public TRepository GetRepository<TRepository>() where TRepository : IRepository
    {
        return _serviceProvider.GetRequiredService<TRepository>();
    }

    /// <summary>
    /// Dispose the underlying DbContext
    /// </summary>
    public void Dispose()
    {
        _dbContext?.Dispose();
    }

    /// <summary>
    /// Dispose the underlying DbContext asynchronously
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_dbContext != null)
        {
            await _dbContext.DisposeAsync();
        }
    }
}