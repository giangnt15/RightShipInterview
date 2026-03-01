using RightShip.Core.Domain.Repositories;
using RightShip.Core.Persistence.EfCore;
using RightShip.OrderService.Domain.Repositories;

namespace RightShip.OrderService.Persistence.EfCore;

/// <summary>
/// Unit of work implementation for Order service, extending EfCoreUow.
/// </summary>
public class OrderUnitOfWork : EfCoreUow<OrderDbContext, OrderDbContextFactory>
{
    public OrderUnitOfWork(OrderDbContextFactory dbContextFactory, IServiceProvider serviceProvider)
        : base(dbContextFactory, serviceProvider)
    {
    }

    /// <inheritdoc />
    public override TRepository GetRepository<TRepository>()
    {
        if (typeof(TRepository) == typeof(IOrderRepository))
        {
            if (_dbContext == null)
            {
                throw new InvalidOperationException("Unit of work not started");
            }
            return (TRepository)(object)new OrderRepository(_dbContext);
        }
        return base.GetRepository<TRepository>();
    }
}
