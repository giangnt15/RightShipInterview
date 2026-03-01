using Microsoft.EntityFrameworkCore;
using RightShip.Core.Persistence.EfCore;
using RightShip.ProductService.Domain.Entities;

namespace RightShip.ProductService.Persistence.EfCore;

/// <summary>
/// EF Core database context for Product aggregate and outbox.
/// </summary>
public class ProductDbContext : BaseEfCoreDbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductReservation> ProductReservations => Set<ProductReservation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(b =>
        {
            b.ToTable("Products");
            b.HasKey(p => p.Id);
            b.Property(p => p.Name).HasMaxLength(256).IsRequired();
            b.Property(p => p.CreatedAt);
            b.Property(p => p.CreatedBy);
            b.Property(p => p.UpdatedAt);
            b.Property(p => p.UpdatedBy);
            b.Property(p => p.Version).IsConcurrencyToken().ValueGeneratedNever();
            b.ComplexProperty(p => p.Price, price =>
            {
                price.Property(x => x.Amount).HasColumnName("PriceAmount");
            });
            b.ComplexProperty(p => p.Quantity, q =>
            {
                q.Property(x => x.Value).HasColumnName("QuantityValue");
            });
            b.HasIndex(p => p.Name);
        });

        modelBuilder.Entity<ProductReservation>(b =>
        {
            b.ToTable("ProductReservations");
            b.HasKey(r => r.Id);
            b.Property(r => r.ProductId);
            b.Property(r => r.Quantity);
            b.Property(r => r.Status);
            b.Property(r => r.ExpiresAt);
            b.Property(r => r.CreatedAt);
            b.Property(r => r.Version).IsConcurrencyToken().ValueGeneratedNever();
            b.HasIndex(r => new { r.ProductId, r.Status });
            b.HasIndex(r => r.ExpiresAt);
        });
    }
}
