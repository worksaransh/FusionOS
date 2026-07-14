using System.Linq.Expressions;
using System.Text.Json;
using FusionOS.SharedKernel;
using FusionOS.SharedKernel.Auditing;
using FusionOS.SharedKernel.Context;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// Base DbContext every module's own DbContext derives from. Applies the global
/// soft-delete query filter, stamps audit columns on save, stages outbox
/// messages for any aggregate's domain events, and provides the
/// ProcessedIntegrationEvents idempotency ledger consumers use — so no module
/// has to reimplement this plumbing (04_DATABASE_GUIDELINES.md §3-§5).
/// </summary>
public abstract class BaseDbContext : DbContext
{
    private readonly ICurrentUserContext _currentUser;

    protected BaseDbContext(DbContextOptions options, ICurrentUserContext currentUser) : base(options)
    {
        _currentUser = currentUser;
    }

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ProcessedIntegrationEvent's PK isn't named "Id"/"{Type}Id" by EF
        // convention (it's "EventId" to read naturally at call sites), so it
        // needs an explicit HasKey — every module's own entities keep using
        // convention-based Entity.Id and don't need this.
        modelBuilder.Entity<ProcessedIntegrationEvent>().HasKey(x => x.EventId);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                var property = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var falseConstant = Expression.Constant(false);
                var body = Expression.Equal(property, falseConstant);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(Expression.Lambda(body, parameter));
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges() => SaveChangesAsync().GetAwaiter().GetResult();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAudit();
        StageOutboxMessages();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void StampAudit()
    {
        var now = DateTimeOffset.UtcNow;
        var actor = _currentUser.UserId ?? Guid.Empty;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added) entry.Entity.SetCreationAudit(now, actor);
            else if (entry.State == EntityState.Modified) entry.Entity.SetModificationAudit(now, actor);
        }

        foreach (var entry in ChangeTracker.Entries<TenantAggregateRoot>())
        {
            if (entry.State == EntityState.Added) entry.Entity.SetCreationAudit(now, actor);
            else if (entry.State == EntityState.Modified) entry.Entity.SetModificationAudit(now, actor);
        }
    }

    /// <summary>
    /// Converts any pending domain events on tracked aggregates into outbox rows,
    /// tagged with the aggregate's CompanyId (via ITenantScoped) where available so
    /// the generic OutboxDispatcher (FusionOS.BuildingBlocks.EventBus) can publish
    /// them to Kafka without any module-specific mapping code (03_SYSTEM_ARCHITECTURE.md
    /// §4.2). Aggregates that aren't tenant-scoped (e.g. Company itself) stage with
    /// CompanyId = Guid.Empty — acceptable since those events are platform-level,
    /// not company-scoped, by definition.
    /// </summary>
    private void StageOutboxMessages()
    {
        var aggregatesWithEvents = ChangeTracker.Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        foreach (var aggregate in aggregatesWithEvents)
        {
            var companyId = aggregate is ITenantScoped tenantScoped ? tenantScoped.CompanyId : Guid.Empty;

            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var payload = JsonSerializer.Serialize(domainEvent, domainEvent.GetType());
                OutboxMessages.Add(OutboxMessage.Create(domainEvent.GetType().Name, companyId, payload));
            }

            aggregate.ClearDomainEvents();
        }
    }
}
