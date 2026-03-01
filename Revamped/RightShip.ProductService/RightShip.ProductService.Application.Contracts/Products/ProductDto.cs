namespace RightShip.ProductService.Application.Contracts.Products;

/// <summary>
/// Data transfer object for product read operations.
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}
