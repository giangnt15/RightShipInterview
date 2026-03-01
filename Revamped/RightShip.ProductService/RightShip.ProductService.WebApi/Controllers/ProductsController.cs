using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RightShip.ProductService.Application.Contracts.Products;

namespace RightShip.ProductService.WebApi.Controllers;

/// <summary>
/// In a real system we should have authentication and authorization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductAppService _productAppService;

    public ProductsController(IProductAppService productAppService)
    {
        _productAppService = productAppService;
    }

    [HttpGet("trace-test")]
    public IActionResult TraceTest() =>
        Ok(new { traceId = Activity.Current?.TraceId.ToString() ?? "none" });

    /// <summary>
    /// Get product by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await _productAppService.GetByIdAsync(id, cancellationToken);
        if (product == null)
        {
            return NotFound();
        }
        return Ok(product);
    }

    /// <summary>
    /// Get paged list of products with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetList(
        [FromQuery] string? searchName,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string sortBy = "Name",
        [FromQuery] string sortDirection = "Asc",
        CancellationToken cancellationToken = default)
    {
        var filter = new ProductListFilterDto
        {
            SearchName = searchName,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _productAppService.GetListAsync(filter, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new product. Exceptions are handled by GlobalExceptionHandler.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateProductDto dto,
        [FromHeader(Name = "X-Created-By")] Guid? createdBy,
        CancellationToken cancellationToken)
    {
        var result = await _productAppService.CreateProductAsync(dto, createdBy ?? Guid.Empty, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
