using Microsoft.EntityFrameworkCore;
using RightShip.OrderService.Domain.Entities;
using RightShip.OrderService.Domain.Repositories;

namespace RightShip.OrderService.Persistence.EfCore;

/// <summary>
/// EF Core implementation of order repository.
/// </summary>
public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _context;

    public OrderRepository(OrderDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public async Task<Order> LoadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var order = await _context.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        return order;
    }

    /// <inheritdoc />
    public async Task<Order> AddAsync(Order entity)
    {
        await _context.Orders.AddAsync(entity);
        return entity;
    }

    /// <inheritdoc />
    public Task<Order> UpdateAsync(Order entity, Guid id)
    {
        if (_context.Entry(entity).State == EntityState.Detached)
        {
            _context.Orders.Update(entity);
        }
        return Task.FromResult(entity);
    }

    /// <inheritdoc />
    public async Task<Order> DeleteAsync(Guid id)
    {
        var order = await LoadAsync(id);
        _context.Orders.Remove(order);
        return order;
    }
}
