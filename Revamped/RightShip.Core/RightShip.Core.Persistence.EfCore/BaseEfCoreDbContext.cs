using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using RightShip.Core.Common.Types;
using RightShip.Core.Domain.Entities;

namespace RightShip.Core.Persistence.EfCore;

/// <summary>
/// The base class for the EF Core database context.
/// </summary>
public abstract class BaseEfCoreDbContext : DbContext
{
    protected BaseEfCoreDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureOutboxTable(modelBuilder);
        AddConcurrencyCheck(modelBuilder);
    }


    /// <summary>
    /// Configure the outbox table to ensure at least once delivery of messages.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected virtual void ConfigureOutboxTable(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OutboxMessage>(builder =>
        {
            builder.ToTable("outbox_message");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Topic).HasMaxLength(255);
            builder.Property(x => x.Payload).HasColumnType("longtext");

            builder.HasIndex(x => x.CreatedAt);
            builder.HasIndex(x => x.Sent);
            builder.HasIndex(x => x.UpdatedAt);
            builder.HasIndex(x => x.CorrelationId);
            builder.HasIndex(x => x.Processing);

        });
    }

    /// <summary>
    /// Add concurrency check to the model builder.
    /// This is used to prevent race conditions when multiple users update the same entity concurrently.
    /// Useful in case 2 orders update the same product quantity at the same time. It acts as a optimistic locking mechanism.
    /// </summary>
    /// <param name="modelBuilder">The model builder.</param>
    protected virtual void AddConcurrencyCheck(ModelBuilder modelBuilder)
    {
        var checkTypes = TypeHelper.GetAllClassSubTypes<IVersioned>();
        if (checkTypes != null && checkTypes.Any())
        {
            foreach (var type in checkTypes)
            {
                if (type.Name.Contains("AggregateRoot") || type.Name.Contains("BaseEntity")) continue;
                modelBuilder.Entity(type, action =>
                {
                    action.Property("Version")
                        .IsRowVersion()
                        .IsConcurrencyToken();
                });
            }
        }
    }

    /// <summary>
    /// Save the changes to the database.
    /// This method is overridden to ensure that the outbox messages are saved to the database in the same transaction as the aggregate roots.
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    public override int SaveChanges()
    {
        var changedEntries = GetChangedEntities();
        var aggregateRootEntries = GetChangedAggregateRootEntries(changedEntries);
        var outboxMsgs = GetOutboxMessages(aggregateRootEntries);
        if (outboxMsgs.Count > 0)
        {
            AddRange(outboxMsgs);
            foreach (var outboxMsg in outboxMsgs)
            {
                var entry = Entry(outboxMsg);
                if (entry.State != EntityState.Added)
                {
                    entry.State = EntityState.Added;
                }
            }
        }
        ChangeTracker.DetectChanges();
        var affected = base.SaveChanges();
        aggregateRootEntries.ForEach(x => (x.Entity as IAggregateRoot)?.ClearDistributedEvents());
        return affected;
    }
    
    /// <summary>
    /// Save the changes to the database.
    /// This method is overridden to ensure that the outbox messages are saved to the database in the same transaction as the aggregate roots.
    /// </summary>
    /// <returns>The number of affected rows.</returns>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changedEntries = GetChangedEntities();
        var aggregateRootEntries = GetChangedAggregateRootEntries(changedEntries);
        var outboxMsgs = GetOutboxMessages(aggregateRootEntries);
        if (outboxMsgs.Count > 0)
        {
            AddRange(outboxMsgs);
            foreach (var outboxMsg in outboxMsgs)
            {
                var entry = Entry(outboxMsg);
                if (entry.State != EntityState.Added)
                {
                    entry.State = EntityState.Added;
                }
            }
        }
        ChangeTracker.DetectChanges();
        var affected = await base.SaveChangesAsync(cancellationToken);
        aggregateRootEntries.ForEach(x => (x.Entity as IAggregateRoot)?.ClearDistributedEvents());
        return affected;
    }

    /// <summary>
    /// Get the outbox messages for the aggregate roots.
    /// </summary>
    /// <param name="aggregateRoots">The aggregate roots.</param>
    /// <returns>The outbox messages.</returns>

    private static List<OutboxMessage> GetOutboxMessages(List<EntityEntry> aggregateRoots)
    {
        var outboxMsgs = new List<OutboxMessage>();
        if (aggregateRoots.Count > 0)
        {
            foreach (var aggregateRootEntry in aggregateRoots)
            {
                if (aggregateRootEntry.Entity is not IAggregateRoot aggregateRoot) continue;
                var events = aggregateRoot.GetDistributedEvents();
                if (events.Any())
                {
                    foreach (var evt in events)
                    {
                        outboxMsgs.Add(new OutboxMessage()
                        {
                            CorrelationId = evt.SourceId.ToString()!,
                            Payload = JsonSerializer.Serialize(evt),
                            CreatedAt = evt.Timestamp.UtcDateTime,
                            Topic = aggregateRoot.GetEventTopic(),
                        });
                    }
                }
            }
        }
        return outboxMsgs;
    }
    /// <summary>
    /// Get the changed aggregate root entries.
    /// </summary>
    /// <param name="entries">The entries.</param>
    /// <returns>The changed aggregate root entries.</returns>
    private static List<EntityEntry> GetChangedAggregateRootEntries(IEnumerable<EntityEntry> entries)
    {
        var aggs = new List<EntityEntry>();
        foreach (var item in entries)
        {
            if (item.Entity is IAggregateRoot)
            {
                aggs.Add(item);
            }
        }
        return aggs;
    }
    /// <summary>
    /// Get the changed entities.
    /// </summary>
    /// <returns>The changed entities.</returns>

    private IEnumerable<EntityEntry> GetChangedEntities()
    {
        return ChangeTracker.Entries().Where(x => x.State != EntityState.Unchanged && x.State != EntityState.Detached);
    }
}