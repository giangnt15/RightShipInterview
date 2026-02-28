using RightShip.Core.Domain.Entities;
using RightShip.Core.Domain.ValueObjects;
using RightShip.OrderService.Domain.Exceptions;

namespace RightShip.OrderService.Domain.Entities;

/// <summary>
/// Order line is a child entity of Order aggregate.
/// </summary>
public class OrderLine : BaseEntity<Guid>
{
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; } = Money.Zero();

    public Money SubTotal => UnitPrice * Quantity;

    private OrderLine()
    {
    }

    internal OrderLine(Guid id, Guid productId, int quantity, Money unitPrice)
    {
        if (productId == Guid.Empty)
        {
            throw new OrderLineProductRequiredException();
        }

        if (quantity <= 0)
        {
            throw new OrderLineQuantityMustBePositiveException();
        }

        Id = id;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }

    internal static OrderLine Create(Guid id, Guid productId, int quantity, Money unitPrice)
    {
        return new OrderLine(id, productId, quantity, unitPrice);
    }
}

