using RightShip.Core.Domain.Repositories;
using RightShip.OrderService.Domain.Entities;

namespace RightShip.OrderService.Domain.Repositories;

/// <summary>
/// Repository interface for working with Order aggregate roots.
/// </summary>
public interface IOrderRepository : IRepository<Order, Guid>
{
}

