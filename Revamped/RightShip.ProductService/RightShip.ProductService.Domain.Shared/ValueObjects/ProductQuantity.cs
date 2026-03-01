using RightShip.Core.Domain.ValueObjects;

namespace RightShip.ProductService.Domain.Shared.ValueObjects;

/// <summary>
/// Quantity of a product, constrained to non-negative integers.
/// </summary>
public record class ProductQuantity : BaseValueObject
{
    public int Value { get; init; }

    /// <summary>
    /// For EF Core materialization only.
    /// </summary>
    private ProductQuantity()
    {
    }

    public ProductQuantity(int value)
    {
        if (value < 0)
        {
            throw new ArgumentException("Quantity cannot be negative", nameof(value));
        }

        Value = value;
    }

    public static ProductQuantity Zero() => new(0);

    public static ProductQuantity operator +(ProductQuantity a, ProductQuantity b) =>
        new(a.Value + b.Value);

    public static ProductQuantity operator -(ProductQuantity a, ProductQuantity b)
    {
        var result = a.Value - b.Value;
        if (result < 0)
        {
            throw new ArgumentException("Quantity cannot be negative after subtraction");
        }

        return new ProductQuantity(result);
    }
}

