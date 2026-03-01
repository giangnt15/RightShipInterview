using Microsoft.EntityFrameworkCore;
using RightShip.Core.Observability;
using RightShip.Core.Persistence.EfCore;
using RightShip.OrderService.Application.Contracts.Orders;
using RightShip.OrderService.Application.Orders;
using RightShip.OrderService.Infrastructure.ProductService;
using RightShip.OrderService.Infrastructure.RateLimiting;
using RightShip.OrderService.Persistence.EfCore;
using RightShip.OrderService.WebApi.Middleware;

namespace RightShip.OrderService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();
            builder.Services.AddOpenTelemetryObservability("RightShip.OrderService", builder.Configuration);
            builder.Services.AddOrderCreationRateLimiting(builder.Configuration);

            var productServiceUrl = builder.Configuration["ProductService:Url"] ?? "http://localhost:5118";
            builder.Services.AddHealthChecks()
                .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
                .AddTypeActivatedCheck<RightShip.Core.Persistence.EfCore.DatabaseHealthCheck<OrderDbContext>>("database", failureStatus: null, tags: ["ready"])
                .AddUrlGroup(new Uri($"{productServiceUrl.TrimEnd('/')}/health/ready"), "ProductService", tags: ["ready"]);

            builder.Services.AddOrderPersistence(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db"));
            builder.Services.AddProductServiceClient(
                builder.Configuration["ProductService:Url"] ?? "http://localhost:5118");
            builder.Services.AddScoped<IOrderAppService, OrderAppService>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.EnsureMigrateDb<OrderDbContext, OrderDbContextFactory>();
            app.UseTraceIdResponseHeader();
            app.UseExceptionHandler();
            app.UseRateLimiter();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("live") });
            app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = c => c.Tags.Contains("ready") });

            app.Run();
        }
    }
}
