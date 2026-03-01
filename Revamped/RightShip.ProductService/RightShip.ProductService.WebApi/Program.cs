using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using RightShip.Core.Persistence.EfCore;
using RightShip.ProductService.Application.Contracts.Products;
using RightShip.ProductService.Application.Products;
using RightShip.ProductService.Persistence.EfCore;
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

            // Enable HTTP/2 on HTTP endpoints for gRPC
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ConfigureEndpointDefaults(listenOptions =>
                {
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            builder.Services.AddProductPersistence(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=products.db"));
            builder.Services.AddScoped<IProductAppService, ProductAppService>();

            builder.Services.AddControllers();
            builder.Services.AddGrpc();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            app.EnsureMigrateDb<ProductDbContext, ProductDbContextFactory>();
            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseAuthorization();

            app.MapControllers();
            app.MapGrpcService<ProductGrpcService>();

            app.Run();
        }
    }
}
