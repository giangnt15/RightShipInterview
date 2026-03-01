using System.Net;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.CircuitBreaker;
using Polly.Extensions.Http;
using Polly.Retry;
using Polly.Timeout;
using RightShip.OrderService.Application.Contracts.Integration;
using RightShip.ProductService.Application.Contracts.Grpc;

namespace RightShip.OrderService.Infrastructure.ProductService;

/// <summary>
/// DI registration for Product Service gRPC client with resilience (retry, circuit breaker, timeout).
/// </summary>
public static class ServiceCollectionExtensions
{
    private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout
                or HttpStatusCode.BadGateway or (HttpStatusCode)429)
            .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));
    }

    private static AsyncCircuitBreakerPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode is HttpStatusCode.ServiceUnavailable or HttpStatusCode.GatewayTimeout)
            .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
    }

    private static AsyncTimeoutPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10));
    }

    /// <summary>
    /// Adds Product Service gRPC client with retry, circuit breaker, and timeout. Registers IProductServiceClient adapter.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="productServiceAddress">Product Service gRPC address (e.g. http://localhost:5118).</param>
    public static IServiceCollection AddProductServiceClient(
        this IServiceCollection services,
        string productServiceAddress)
    {
        services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options =>
        {
            options.Address = new Uri(productServiceAddress);
        })
            .AddPolicyHandler(GetTimeoutPolicy())
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());

        services.AddScoped<IProductServiceClient, ProductGrpcClientAdapter>();
        return services;
    }
}
