using RightShip.Core.Persistence.EfCore;

namespace RightShip.OrderService.Persistence.EfCore;

/// <summary>
/// Factory for creating OrderDbContext instances.
/// </summary>
public class OrderDbContextFactory : BaseEfCoreDbContextFactory<OrderDbContext>
{
    public OrderDbContextFactory(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
    }
}
