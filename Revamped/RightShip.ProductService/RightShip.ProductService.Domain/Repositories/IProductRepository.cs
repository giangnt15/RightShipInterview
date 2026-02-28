using RightShip.Core.Domain.Repositories;
using RightShip.ProductService.Domain.Entities;

namespace RightShip.ProductService.Domain.Repositories;

/// <summary>
/// Repository interface for Product aggregate roots.
/// </summary>
public interface IProductRepository : IRepository<Product, Guid>
{
}

