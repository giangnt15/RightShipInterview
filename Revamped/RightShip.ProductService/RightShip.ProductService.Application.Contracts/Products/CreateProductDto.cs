namespace RightShip.ProductService.Application.Contracts.Products;

/// <summary>
/// Input for creating a product.
/// </summary>
public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InitialQuantity { get; set; }
}
