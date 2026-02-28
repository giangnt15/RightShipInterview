using System.Diagnostics.CodeAnalysis;
using RightShip.Core.Domain.Entities;
using RightShip.Core.Domain.Events;
using RightShip.Core.Domain.ValueObjects;
using RightShip.OrderService.Domain.Exceptions;
using RightShip.OrderService.Domain.Events;
using RightShip.OrderService.Domain.Shared.Enums;

namespace RightShip.OrderService.Domain.Entities;

/// <summary>
/// Order aggregate root.
/// Responsible for maintaining consistency of order and its order lines.
/// </summary>
public class Order : AggregateRoot<Guid>, IHasCreationInfo, IHasModificationInfo
{
    private readonly List<OrderLine> _lines = [];

    public Guid CustomerId { get; private set; }
    public OrderStatus Status { get; private set; } = OrderStatus.Draft;

    public DateTime CreatedAt { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdatedBy { get; set; }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    public Money Total { get; private set; } = Money.Zero();

    [SetsRequiredMembers]
    private Order()
    {
    }

    public static Order Create(Guid customerId, IEnumerable<OrderLine>? lines, Guid createdBy)
    {
        if (customerId == Guid.Empty)
        {
            throw new CustomerIdRequiredException();
        }

        var order = new Order();

        var linePayloads = (lines ?? Enumerable.Empty<OrderLine>())
            .Select(l => new OrderLinePayload
            {
                OrderLineId = Guid.NewGuid(),
                ProductId = l.ProductId,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice.Amount
            })
            .ToList();

        var initialTotal = linePayloads.Aggregate(0m, (acc, p) => acc + (p.UnitPrice * p.Quantity));

        var createdEvent = new OrderCreated
        {
            SourceId = Guid.NewGuid(),
            CustomerId = customerId,
            CreatedBy = createdBy,
            Lines = linePayloads,
            Total = initialTotal
        };

        order.Apply(createdEvent);

        return order;
    }

    public void AddLine(Guid productId, int quantity, Money unitPrice)
    {
        AddLines(new[] { (productId, quantity, unitPrice) });
    }

    public void AddLines(IEnumerable<(Guid productId, int quantity, Money unitPrice)> lines)
    {
        var payloads = lines
            .Select(l => new OrderLinePayload
            {
                OrderLineId = Guid.NewGuid(),
                ProductId = l.productId,
                Quantity = l.quantity,
                UnitPrice = l.unitPrice.Amount
            })
            .ToList();

        if (payloads.Count == 0)
        {
            throw new OrderLinesRequiredException();
        }

        var delta = payloads.Aggregate(0m, (acc, p) => acc + (p.UnitPrice * p.Quantity));
        var newTotal = Total.Amount + delta;

        var @event = new OrderLineAdded
        {
            SourceId = Id,
            Lines = payloads,
            Total = newTotal
        };

        Apply(@event);
    }

    public void RemoveLine(Guid lineId, string currency = "USD")
    {
        var existing = _lines.FirstOrDefault(l => l.Id == lineId);
        if (existing == null)
        {
            throw new OrderLineNotFoundException(lineId);
        }

        var newTotal = Total.Amount - existing.SubTotal.Amount;

        var @event = new OrderLineRemoved
        {
            SourceId = Id,
            OrderLineId = lineId,
            Total = newTotal
        };

        Apply(@event);
    }

    protected override void When(IEvent @event)
    {
        switch (@event)
        {
            case OrderCreated created:
                Id = (Guid)created.SourceId;
                CustomerId = created.CustomerId;
                CreatedBy = created.CreatedBy;
                UpdatedBy = created.CreatedBy;
                CreatedAt = created.Timestamp.UtcDateTime;
                UpdatedAt = CreatedAt;
                Total = new Money { Amount = created.Total };

                foreach (var payload in created.Lines)
                {
                    var unitPrice = new Money { Amount = payload.UnitPrice };
                    var createdLine = OrderLine.Create(payload.OrderLineId, payload.ProductId, payload.Quantity, unitPrice);
                    _lines.Add(createdLine);
                }

                Status = OrderStatus.Submitted;
                break;

            case OrderLineAdded added:
                foreach (var payload in added.Lines)
                {
                    var unitPrice = new Money { Amount = payload.UnitPrice };
                    var line = OrderLine.Create(payload.OrderLineId, payload.ProductId, payload.Quantity, unitPrice);
                    _lines.Add(line);
                }
                Total = new Money { Amount = added.Total };
                UpdatedAt = added.Timestamp.UtcDateTime;
                break;

            case OrderLineRemoved removed:
                var existing = _lines.First(l => l.Id == removed.OrderLineId);
                _lines.Remove(existing);
                Total = new Money { Amount = removed.Total };
                UpdatedAt = removed.Timestamp.UtcDateTime;
                break;
        }
    }

    protected override void EnsureValidState()
    {
        if (CustomerId == Guid.Empty)
        {
            throw new CustomerIdRequiredException();
        }

        if (_lines.Count == 0)
        {
            throw new OrderMustHaveAtLeastOneLineException();
        }

        if (Total.Amount < 0)
        {
            throw new NegativeOrderTotalException();
        }
    }
}

