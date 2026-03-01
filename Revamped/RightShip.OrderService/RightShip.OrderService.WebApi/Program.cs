using Microsoft.EntityFrameworkCore;
using RightShip.Core.Persistence.EfCore;
using RightShip.OrderService.Application.Contracts.Orders;
using RightShip.OrderService.Application.Orders;
using RightShip.OrderService.Persistence.EfCore;

namespace RightShip.OrderService.WebApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddOrderPersistence(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=orders.db"));
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
            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
