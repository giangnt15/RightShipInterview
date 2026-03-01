using Grpc.Core;
using RightShip.Core.Application.Uow;
using RightShip.ProductService.Application.Contracts.Grpc;
using RightShip.ProductService.Domain.Exceptions;
using RightShip.ProductService.Domain.Repositories;

namespace RightShip.ProductService.Application.Products;

/// <summary>
/// gRPC service for product price lookup and stock reservation (internal service-to-service communication).
/// </summary>
public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductGrpcService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public override async Task<GetProductPriceResponse> GetProductPrice(
        GetProductPriceRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product id."));
        }

        try
        {
            await _unitOfWork.StartAsync(context.CancellationToken);
            var repo = _unitOfWork.GetRepository<IProductRepository>();
            var product = await repo.LoadAsync(productId, context.CancellationToken);
            return new GetProductPriceResponse
            {
                ProductId = product.Id.ToString(),
                Price = (double)product.Price.Amount
            };
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product '{productId}' not found."));
        }
    }

    public override async Task<ReserveStockResponse> ReserveStock(
        ReserveStockRequest request,
        ServerCallContext context)
    {
        if (!Guid.TryParse(request.ProductId, out var productId))
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid product id."));
        }

        if (request.Quantity <= 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "Quantity must be positive."));
        }

        try
        {
            await _unitOfWork.StartAsync(context.CancellationToken);
            var repo = _unitOfWork.GetRepository<IProductRepository>();
            var product = await repo.LoadAsync(productId, context.CancellationToken);
            // We could use a more sophisticated approach to reserve stock via a queue such as Product Bucket, Eventual Consistency with Reservation Aggregate
            // to avoid hot products from being updated too frequently but for simplicity here we just adjust the quantity and use atomic update
            // via Version column with the help of EfCore.
            product.AdjustQuantity(-request.Quantity);
            await repo.UpdateAsync(product, productId);
            await _unitOfWork.CommitAsync(context.CancellationToken);
            return new ReserveStockResponse();
        }
        catch (ProductQuantityMustBeNonNegativeException)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Insufficient stock."));
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product '{productId}' not found."));
        }
    }
}
