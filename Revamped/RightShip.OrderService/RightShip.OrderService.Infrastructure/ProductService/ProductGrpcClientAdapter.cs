using Grpc.Net.Client;
using RightShip.OrderService.Application.Contracts.Integration;
using RightShip.ProductService.Application.Contracts.Grpc;

namespace RightShip.OrderService.Infrastructure.ProductService;

/// <summary>
/// gRPC client adapter for Product Service.
/// </summary>
public class ProductGrpcClientAdapter : IProductServiceClient
{
    private readonly ProductGrpc.ProductGrpcClient _client;

    public ProductGrpcClientAdapter(ProductGrpc.ProductGrpcClient client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<decimal> GetProductPriceAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        var request = new GetProductPriceRequest { ProductId = productId.ToString() };
        try
        {
            var response = await _client.GetProductPriceAsync(request, cancellationToken: cancellationToken);
            return (decimal)response.Price;
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            throw new ProductNotFoundException(productId);
        }
    }

    /// <inheritdoc />
    public async Task<Guid> CreateReservationAsync(Guid productId, int quantity, int? ttlSeconds = null, CancellationToken cancellationToken = default)
    {
        var request = new CreateReservationRequest
        {
            ProductId = productId.ToString(),
            Quantity = quantity
        };
        if (ttlSeconds.HasValue && ttlSeconds.Value > 0)
        {
            request.TtlSeconds = ttlSeconds.Value;
        }
        try
        {
            var response = await _client.CreateReservationAsync(request, cancellationToken: cancellationToken);
            return Guid.Parse(response.ReservationId);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.NotFound)
        {
            throw new ProductNotFoundException(productId);
        }
        catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.FailedPrecondition)
        {
            throw new InsufficientStockException(productId, quantity);
        }
    }

    /// <inheritdoc />
    public async Task ConfirmReservationsAsync(IReadOnlyList<Guid> reservationIds, CancellationToken cancellationToken = default)
    {
        if (reservationIds.Count == 0)
        {
            return;
        }
        var request = new ConfirmReservationsRequest();
        foreach (var id in reservationIds)
        {
            request.ReservationIds.Add(id.ToString());
        }
        await _client.ConfirmReservationsAsync(request, cancellationToken: cancellationToken);
    }
}
