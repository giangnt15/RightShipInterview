namespace RightShip.ProductService.Application.Contracts.Products;

/// <summary>
/// Filter for product list query.
/// </summary>
public class ProductListFilterDto
{
    /// <summary>
    /// Optional search by product name (contains, case-insensitive).
    /// </summary>
    public string? SearchName { get; set; }

    /// <summary>
    /// Page number (1-based). Default 1.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size. Default 10, max 100.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Sort field: Name, Price, Quantity, CreatedAt. Default Name.
    /// </summary>
    public string SortBy { get; set; } = "Name";

    /// <summary>
    /// Sort direction: Asc, Desc. Default Asc.
    /// </summary>
    public string SortDirection { get; set; } = "Asc";
}
