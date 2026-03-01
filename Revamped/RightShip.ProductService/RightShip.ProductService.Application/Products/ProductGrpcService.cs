using Grpc.Core;
using Microsoft.Extensions.Options;
using RightShip.Core.Application.Uow;
using RightShip.ProductService.Application.Contracts.Grpc;
using RightShip.ProductService.Application.Options;
using RightShip.ProductService.Domain.Entities;
using RightShip.ProductService.Domain.Repositories;
using RightShip.ProductService.Domain.Exceptions;
using RightShip.ProductService.Domain.Services;

namespace RightShip.ProductService.Application.Products;

/// <summary>
/// gRPC service for product price lookup and reservation (internal service-to-service communication).
/// Uses ProductReservation aggregate with TTL; reservations expire if Order Service fails to confirm.
/// </summary>
public class ProductGrpcService : ProductGrpc.ProductGrpcBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ReservationOptions _reservationOptions;
    private readonly IReservationConfirmationService _reservationConfirmationService;

    public ProductGrpcService(
        IUnitOfWork unitOfWork,
        IOptions<ReservationOptions> reservationOptions,
        IReservationConfirmationService reservationConfirmationService)
    {
        _unitOfWork = unitOfWork;
        _reservationOptions = reservationOptions.Value;
        _reservationConfirmationService = reservationConfirmationService;
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

    public override async Task<CreateReservationResponse> CreateReservation(
        CreateReservationRequest request,
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

        var ttlSeconds = request.TtlSeconds > 0 ? request.TtlSeconds : _reservationOptions.DefaultTtlSeconds;
        var ttl = TimeSpan.FromSeconds(ttlSeconds);

        try
        {
            await _unitOfWork.StartAsync(context.CancellationToken);
            var productRepo = _unitOfWork.GetRepository<IProductRepository>();
            var reservationRepo = _unitOfWork.GetRepository<IProductReservationRepository>();

            var product = await productRepo.LoadAsync(productId, context.CancellationToken);
            var pendingQty = await reservationRepo.GetTotalPendingQuantityForProductAsync(productId, context.CancellationToken);
            var available = product.Quantity.Value - pendingQty;

            if (request.Quantity > available)
            {
                throw new RpcException(new Status(StatusCode.FailedPrecondition, "Insufficient available stock."));
            }

            var reservation = ProductReservation.Create(productId, request.Quantity, ttl);
            await reservationRepo.AddAsync(reservation);
            await _unitOfWork.CommitAsync(context.CancellationToken);

            return new CreateReservationResponse
            {
                ReservationId = reservation.Id.ToString(),
                ExpiresAt = reservation.ExpiresAt.ToString("O")
            };
        }
        catch (RpcException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw new RpcException(new Status(StatusCode.NotFound, $"Product '{productId}' not found."));
        }
    }

    public override async Task<ConfirmReservationsResponse> ConfirmReservations(
        ConfirmReservationsRequest request,
        ServerCallContext context)
    {
        if (request.ReservationIds.Count == 0)
        {
            return new ConfirmReservationsResponse();
        }

        var ids = new List<Guid>();
        foreach (var idStr in request.ReservationIds)
        {
            if (Guid.TryParse(idStr, out var id))
            {
                ids.Add(id);
            }
        }

        if (ids.Count == 0)
        {
            throw new RpcException(new Status(StatusCode.InvalidArgument, "No valid reservation ids."));
        }

        try
        {
            await _unitOfWork.StartAsync(context.CancellationToken);
            var reservationRepo = _unitOfWork.GetRepository<IProductReservationRepository>();
            var productRepo = _unitOfWork.GetRepository<IProductRepository>();

            foreach (var id in ids)
            {
                var reservation = await reservationRepo.LoadAsync(id, context.CancellationToken);
                var product = await productRepo.LoadAsync(reservation.ProductId, context.CancellationToken);
                _reservationConfirmationService.ConfirmReservation(reservation, product);
                await reservationRepo.UpdateAsync(reservation, id);
                await productRepo.UpdateAsync(product, product.Id);
            }

            await _unitOfWork.CommitAsync(context.CancellationToken);
            return new ConfirmReservationsResponse();
        }
        catch (ReservationExpiredException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (ReservationAlreadyConfirmedException ex)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, ex.Message));
        }
        catch (ProductQuantityMustBeNonNegativeException)
        {
            throw new RpcException(new Status(StatusCode.FailedPrecondition, "Insufficient stock for confirmation."));
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            throw new RpcException(new Status(StatusCode.NotFound, ex.Message));
        }
    }
}
