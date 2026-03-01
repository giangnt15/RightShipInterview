using Microsoft.EntityFrameworkCore;
using RightShip.Core.Persistence.EfCore;
using RightShip.OrderService.Domain.Entities;

namespace RightShip.OrderService.Persistence.EfCore;

/// <summary>
/// EF Core database context for Order aggregate and outbox.
/// </summary>
public class OrderDbContext : BaseEfCoreDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.CustomerId);
            entity.Property(o => o.Status).HasConversion<int>();
            entity.Property(o => o.CreatedAt);
            entity.Property(o => o.CreatedBy);
            entity.Property(o => o.UpdatedAt);
            entity.Property(o => o.UpdatedBy);
            entity.Property(o => o.Version).IsConcurrencyToken().ValueGeneratedNever();
            entity.ComplexProperty(o => o.Total, total =>
            {
                total.Property(x => x.Amount).HasColumnName("TotalAmount");
            });

            entity.HasMany(o => o.Lines)
                .WithOne()
                .HasForeignKey("OrderId")
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
                
            // Set backing field to allow EF Core to access the private field directly.
            entity.Navigation(o => o.Lines).HasField("_lines");

            entity.HasIndex(o => o.CustomerId);
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.ToTable("OrderLines");
            entity.Property(l => l.ProductId);
            entity.Property(l => l.Quantity);
            entity.ComplexProperty(l => l.UnitPrice, up =>
            {
                up.Property(x => x.Amount).HasColumnName("UnitPriceAmount");
            });
        });
    }
}
