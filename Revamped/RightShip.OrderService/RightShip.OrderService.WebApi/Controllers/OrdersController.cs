using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RightShip.OrderService.Application.Contracts.Orders;

namespace RightShip.OrderService.WebApi.Controllers;

/// <summary>
/// Order API controller.
/// Exceptions are handled by GlobalExceptionHandler.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderAppService _orderAppService;

    public OrdersController(IOrderAppService orderAppService)
    {
        _orderAppService = orderAppService;
    }

    [HttpGet("trace-test")]
    public IActionResult TraceTest() =>
        Ok(new { traceId = Activity.Current?.TraceId.ToString() ?? "none" });

    /// <summary>
    /// Get order by id.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var order = await _orderAppService.GetByIdAsync(id, cancellationToken);
        if (order == null)
        {
            return NotFound();
        }
        return Ok(order);
    }

    /// <summary>
    /// Create a new order. Validates products and reserves stock via Product Service.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOrderDto dto,
        [FromHeader(Name = "X-Created-By")] Guid? createdBy,
        CancellationToken cancellationToken)
    {
        var result = await _orderAppService.CreateOrderAsync(dto, createdBy ?? Guid.Empty, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }
}
