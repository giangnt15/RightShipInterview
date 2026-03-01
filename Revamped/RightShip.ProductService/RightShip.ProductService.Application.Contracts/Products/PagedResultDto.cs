namespace RightShip.ProductService.Application.Contracts.Products;

/// <summary>
/// Paged result wrapper.
/// </summary>
/// <typeparam name="T">Item type.</typeparam>
public class PagedResultDto<T>
{
    /// <summary>
    /// List of items.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = Array.Empty<T>();

    /// <summary>
    /// Total count of items matching the filter.
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Page size.
    /// </summary>
    public int PageSize { get; set; }
}
