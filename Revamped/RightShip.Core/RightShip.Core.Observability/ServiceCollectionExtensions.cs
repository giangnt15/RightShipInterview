using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace RightShip.Core.Observability;

/// <summary>
/// OpenTelemetry observability extensions. Shared by Order and Product services.
/// Adds tracing and context propagation across HTTP/gRPC boundaries.
/// Exports traces to Grafana Alloy/Tempo when OtlpEndpoint is configured.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string OtlpEndpointKey = "OpenTelemetry:OtlpEndpoint";
    private const string OtlpCertificatePathKey = "OpenTelemetry:OtlpCertificatePath";

    /// <summary>
    /// Adds OpenTelemetry tracing with ASP.NET Core, HTTP, and gRPC instrumentation.
    /// Propagates W3C trace context for distributed tracing.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="serviceName">Service name (e.g. "RightShip.OrderService", "RightShip.ProductService").</param>
    /// <param name="configuration">Optional configuration to read OtlpEndpoint (OpenTelemetry:OtlpEndpoint or OTEL_EXPORTER_OTLP_ENDPOINT).</param>
    /// <param name="configure">Optional additional configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        string serviceName,
        IConfiguration? configuration = null,
        Action<TracerProviderBuilder>? configure = null)
    {
        var otlpEndpoint = configuration?[OtlpEndpointKey]
            ?? configuration?["OTEL_EXPORTER_OTLP_ENDPOINT"];
        var otlpCertPath = configuration?[OtlpCertificatePathKey]
            ?? configuration?["OTEL_EXPORTER_OTLP_CERTIFICATE"];

        // Resolve relative cert path to absolute (relative to app base directory)
        if (!string.IsNullOrWhiteSpace(otlpCertPath))
        {
            var fullPath = Path.IsPathRooted(otlpCertPath)
                ? otlpCertPath
                : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, otlpCertPath));
            if (File.Exists(fullPath))
            {
                Environment.SetEnvironmentVariable("OTEL_EXPORTER_OTLP_CERTIFICATE", fullPath);
            }
        }

        var uri = !string.IsNullOrWhiteSpace(otlpEndpoint) ? new Uri(otlpEndpoint) : null;
        var useGrpc = uri != null && (
            uri.Scheme == "https" ||
            string.Equals(configuration?["OpenTelemetry:OtlpProtocol"], "grpc", StringComparison.OrdinalIgnoreCase));

        if (uri != null && uri.Scheme == "http" && useGrpc)
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddGrpcClientInstrumentation()
                    .AddConsoleExporter();

                if (uri != null)
                {
                    tracing.AddOtlpExporter(options =>
                    {
                        options.Endpoint = uri;
                        options.Protocol = useGrpc ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
                    });
                }

                configure?.Invoke(tracing);
            })
            .WithLogging(logging =>
            {
#if DEBUG
                logging.AddConsoleExporter();
#endif
                if (uri != null)
                {
                    logging.AddOtlpExporter(options =>
                    {
                        options.Endpoint = uri;
                        options.Protocol = useGrpc ? OtlpExportProtocol.Grpc : OtlpExportProtocol.HttpProtobuf;
                    });
                }
            });

        return services;
    }
}
