using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RightShip.OrderService.Infrastructure.RateLimiting;

/// <summary>
/// DI registration for order creation rate limiting (per X-Created-By header).
/// </summary>
public static class ServiceCollectionExtensions
{
    private sealed class RateLimitLogger { }

    private const string CreatedByHeader = "X-Created-By";

    /// <summary>
    /// Adds rate limiting for order creation. Partitioned by X-Created-By header; falls back to IP when absent. Returns 429 when limit exceeded.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Configuration (reads RateLimiting:OrderCreation:PermitLimit, WindowSeconds).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOrderCreationRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var permitLimit = configuration.GetValue("RateLimiting:OrderCreation:PermitLimit", 10);
        var windowSeconds = configuration.GetValue("RateLimiting:OrderCreation:WindowSeconds", 60);
        var window = TimeSpan.FromSeconds(windowSeconds);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("OrderCreation", context =>
            {
                var partitionKey = context.Request.Headers[CreatedByHeader].FirstOrDefault()
                    ?? context.Connection.RemoteIpAddress?.ToString()
                    ?? "unknown";
                return RateLimitPartition.GetSlidingWindowLimiter(partitionKey, _ => new SlidingWindowRateLimiterOptions
                {
                    PermitLimit = permitLimit,
                    Window = window,
                    SegmentsPerWindow = 2
                });
            });
            options.OnRejected = (ctx, _) =>
            {
                ctx.HttpContext.Response.Headers.RetryAfter = windowSeconds.ToString();
                var logger = ctx.HttpContext.RequestServices.GetRequiredService<ILogger<RateLimitLogger>>();
                logger.LogWarning("Rate limit exceeded for {Path} (partition: {PartitionKey})",
                    ctx.HttpContext.Request.Path,
                    ctx.HttpContext.Request.Headers[CreatedByHeader].FirstOrDefault()
                        ?? ctx.HttpContext.Connection.RemoteIpAddress?.ToString()
                        ?? "unknown");
                return ValueTask.CompletedTask;
            };
        });

        return services;
    }
}
