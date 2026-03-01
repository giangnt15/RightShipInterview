using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace RightShip.Core.Persistence.EfCore;

/// <summary>
/// Health check for database connectivity using IDbContextFactory (compatible with AddPooledDbContextFactory).
/// </summary>
public sealed class DatabaseHealthCheck<TContext> : IHealthCheck
    where TContext : DbContext
{
    private readonly IDbContextFactory<TContext> _factory;

    public DatabaseHealthCheck(IDbContextFactory<TContext> factory) => _factory = factory;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var dbContext = await _factory.CreateDbContextAsync(cancellationToken);
            return await dbContext.Database.CanConnectAsync(cancellationToken)
                ? HealthCheckResult.Healthy("Database connection is healthy")
                : HealthCheckResult.Unhealthy("Database connection failed");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Database check failed", ex);
        }
    }
}
