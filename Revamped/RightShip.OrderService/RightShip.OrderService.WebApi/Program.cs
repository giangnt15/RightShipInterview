using Microsoft.EntityFrameworkCore;
using RightShip.Core.Persistence.EfCore;
using RightShip.OrderService.Application.Contracts.Orders;
using RightShip.OrderService.Application.Orders;
using RightShip.OrderService.Infrastructure.ProductService;
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
            app.UseExceptionHandler();
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
