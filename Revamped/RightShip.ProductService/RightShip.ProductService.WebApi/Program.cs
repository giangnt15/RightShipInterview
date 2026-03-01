using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using RightShip.Core.Observability;
using RightShip.Core.Persistence.EfCore;
using RightShip.ProductService.Application.Contracts.Products;
using RightShip.ProductService.Application.Products;
using RightShip.ProductService.Persistence.EfCore;
using RightShip.ProductService.Application.Options;
using RightShip.ProductService.Domain;
using RightShip.ProductService.WebApi.HostedServices;
using RightShip.ProductService.WebApi.Middleware;

namespace RightShip.ProductService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddOpenTelemetryObservability("RightShip.ProductService", builder.Configuration);

            // Enable HTTP/2 on HTTP endpoints for gRPC
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            builder.Services.Configure<ReservationOptions>(
                builder.Configuration.GetSection(ReservationOptions.SectionName));

            builder.Services.AddProductDomainServices();

            builder.Services.AddProductPersistence(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=products.db"));
            builder.Services.AddScoped<IProductAppService, ProductAppService>();

            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
                .AddTypeActivatedCheck<RightShip.Core.Persistence.EfCore.DatabaseHealthCheck<ProductDbContext>>("database", failureStatus: null, tags: ["ready"]);

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddHostedService<ExpiredReservationReleaseService>();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.EnsureMigrateDb<ProductDbContext, ProductDbContextFactory>();
            app.UseTraceIdResponseHeader();
            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<ProductGrpcService>();
            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("live") });
            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

            app.Run();
        }
    }
}
