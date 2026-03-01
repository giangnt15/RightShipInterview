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
    public async Task ReserveStockAsync(Guid productId, int quantity, CancellationToken cancellationToken = default)
    {
        var request = new ReserveStockRequest
        {
            ProductId = productId.ToString(),
            Quantity = quantity
        };
        try
        {
            await _client.ReserveStockAsync(request, cancellationToken: cancellationToken);
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
}
