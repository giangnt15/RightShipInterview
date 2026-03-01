using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RightShip.Core.Persistence.EfCore;

namespace RightShip.ProductService.Persistence.EfCore;

/// <summary>
/// Factory for creating ProductDbContext instances (used with pooled DbContext).
/// </summary>
public class ProductDbContextFactory : BaseEfCoreDbContextFactory<ProductDbContext>
{
    public ProductDbContextFactory(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
