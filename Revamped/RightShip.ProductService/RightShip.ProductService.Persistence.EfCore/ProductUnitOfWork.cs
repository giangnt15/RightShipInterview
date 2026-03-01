using RightShip.Core.Domain.Repositories;
using RightShip.Core.Persistence.EfCore;
using RightShip.ProductService.Domain.Repositories;

namespace RightShip.ProductService.Persistence.EfCore;

/// <summary>
/// Unit of work implementation for Product service, extending EfCoreUow.
/// </summary>
public class ProductUnitOfWork : EfCoreUow<ProductDbContext, ProductDbContextFactory>
{
    public ProductUnitOfWork(ProductDbContextFactory dbContextFactory, IServiceProvider serviceProvider)
        : base(dbContextFactory, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override TRepository GetRepository<TRepository>()
    {
        if (_dbContext == null)
        {
            throw new InvalidOperationException("Unit of work not started");
        }

        if (typeof(TRepository) == typeof(IProductRepository))
        {
            return (TRepository)(object)new ProductRepository(_dbContext);
        }

        if (typeof(TRepository) == typeof(IProductReservationRepository))
        {
            return (TRepository)(object)new ProductReservationRepository(_dbContext);
        }

        return base.GetRepository<TRepository>();
    }
}
