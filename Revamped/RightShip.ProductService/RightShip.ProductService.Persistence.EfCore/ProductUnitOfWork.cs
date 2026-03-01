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
        if (typeof(TRepository) == typeof(IProductRepository))
        {
            if (_dbContext == null)
            {
                throw new InvalidOperationException("Unit of work not started");
            }
            return (TRepository)(object)new ProductRepository(_dbContext);
        }
        return base.GetRepository<TRepository>();
    }
}
