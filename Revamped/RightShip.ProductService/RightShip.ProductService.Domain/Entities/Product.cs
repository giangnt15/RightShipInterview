using System.Diagnostics.CodeAnalysis;
using RightShip.Core.Domain.Entities;
using RightShip.Core.Domain.Events;
using RightShip.Core.Domain.ValueObjects;
using RightShip.ProductService.Domain.Events;
using RightShip.ProductService.Domain.Exceptions;
using RightShip.ProductService.Domain.Shared.ValueObjects;

namespace RightShip.ProductService.Domain.Entities;

/// <summary>
/// Product aggregate root.
/// </summary>
public class Product : AggregateRoot<Guid>, IHasCreationInfo, IHasModificationInfo
{
    public string Name { get; private set; } = string.Empty;

    public Money Price { get; private set; } = Money.Zero();

    public ProductQuantity Quantity { get; private set; } = ProductQuantity.Zero();

    public DateTime CreatedAt { get; set; }

    public Guid CreatedBy { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Guid UpdatedBy { get; set; }

    [SetsRequiredMembers]
    private Product()
    {
    }

    public static Product Create(string name, Money price, ProductQuantity initialQuantity, Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ProductNameRequiredException();
        }

        if (price.Amount < 0)
        {
            throw new ProductPriceMustBeNonNegativeException();
        }

        if (initialQuantity.Value < 0)
        {
            throw new ProductQuantityMustBeNonNegativeException();
        }

        var product = new Product();
        var productId = Guid.NewGuid();

        var createdEvent = new ProductCreated
        {
            SourceId = productId,
            ProductId = productId,
            Name = name,
            Price = price.Amount,
            Quantity = initialQuantity.Value
        };

        product.Apply(createdEvent);
        return product;
    }

    public void ChangePrice(Money newPrice)
    {
        if (newPrice.Amount < 0)
        {
            throw new ProductPriceMustBeNonNegativeException();
        }

        var @event = new ProductPriceChanged
        {
            SourceId = Id,
            NewPrice = newPrice.Amount
        };

        Apply(@event);
    }

    public void AdjustQuantity(int delta)
    {
        var newQuantity = Quantity.Value + delta;
        if (newQuantity < 0)
        {
            throw new ProductQuantityMustBeNonNegativeException();
        }

        var @event = new ProductQuantityAdjusted
        {
            SourceId = Id,
            Delta = delta
        };

        Apply(@event);
    }

    protected override void When(IEvent @event)
    {
        switch (@event)
        {
            case ProductCreated created:
                Id = created.ProductId;
                Name = created.Name;
                Price = new Money { Amount = created.Price };
                Quantity = new ProductQuantity(created.Quantity);
                CreatedAt = created.Timestamp.UtcDateTime;
                UpdatedAt = CreatedAt;
                CreatedBy = created.PerformedBy ?? Guid.Empty;
                UpdatedBy = CreatedBy;
                break;

            case ProductPriceChanged priceChanged:
                Price = new Money { Amount = priceChanged.NewPrice };
                UpdatedAt = priceChanged.Timestamp.UtcDateTime;
                UpdatedBy = priceChanged.PerformedBy ?? UpdatedBy;
                break;

            case ProductQuantityAdjusted quantityAdjusted:
                Quantity = new ProductQuantity(Quantity.Value + quantityAdjusted.Delta);
                UpdatedAt = quantityAdjusted.Timestamp.UtcDateTime;
                UpdatedBy = quantityAdjusted.PerformedBy ?? UpdatedBy;
                break;
        }
    }

    protected override void EnsureValidState()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            throw new ProductNameRequiredException();
        }

        if (Price.Amount < 0)
        {
            throw new ProductPriceMustBeNonNegativeException();
        }

        if (Quantity.Value < 0)
        {
            throw new ProductQuantityMustBeNonNegativeException();
        }
    }
}

