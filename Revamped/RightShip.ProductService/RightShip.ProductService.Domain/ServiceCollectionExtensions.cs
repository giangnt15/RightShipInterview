using Microsoft.Extensions.DependencyInjection;
using RightShip.ProductService.Domain.Services;

namespace RightShip.ProductService.Domain;

/// <summary>
/// DI registration for Product Service domain layer.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds domain services (e.g. IReservationConfirmationService).
    /// </summary>
    public static IServiceCollection AddProductDomainServices(this IServiceCollection services)
    {
        services.AddScoped<IReservationConfirmationService, ReservationConfirmationService>();
        return services;
    }
}
