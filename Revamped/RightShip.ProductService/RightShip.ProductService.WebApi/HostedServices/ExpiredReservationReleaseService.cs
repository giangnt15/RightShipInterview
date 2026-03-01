using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RightShip.Core.Application.Uow;
using RightShip.ProductService.Domain.Repositories;

namespace RightShip.ProductService.WebApi.HostedServices;

/// <summary>
/// Background service that periodically releases expired product reservations.
/// Prevents stock from being locked forever when Order Service fails to commit.
/// </summary>
public class ExpiredReservationReleaseService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExpiredReservationReleaseService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    private const int BatchSize = 100;

    public ExpiredReservationReleaseService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExpiredReservationReleaseService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                await unitOfWork.StartAsync(stoppingToken);
                var repo = unitOfWork.GetRepository<IProductReservationRepository>();
                var expired = await repo.GetExpiredPendingReservationsAsync(BatchSize, stoppingToken);

                foreach (var r in expired)
                {
                    r.MarkExpired();
                    await repo.UpdateAsync(r, r.Id);
                }

                if (expired.Count > 0)
                {
                    await unitOfWork.CommitAsync(stoppingToken);
                    _logger.LogInformation("Released {Count} expired reservations", expired.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error releasing expired reservations");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
