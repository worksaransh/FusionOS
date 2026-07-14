using FusionOS.SharedKernel.Auditing;

namespace FusionOS.SharedKernel;

/// <summary>
/// Base for entities that are audited and soft-deletable but are NOT themselves
/// tenant-scoped (e.g. Company, the tenant root). Most entities should instead
/// derive from <see cref="TenantAggregateRoot"/>.
/// </summary>
public abstract class AuditableEntity : Entity, IAuditable, ISoftDeletable, IHasRowVersion
{
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public byte[]? RowVersion { get; private set; }

    /// <summary>Called only by infrastructure (SaveChanges interceptor) — never by domain logic.</summary>
    public void SetCreationAudit(DateTimeOffset at, Guid by)
    {
        CreatedAt = at;
        CreatedBy = by;
    }

    public void SetModificationAudit(DateTimeOffset at, Guid by)
    {
        UpdatedAt = at;
        UpdatedBy = by;
    }

    public void MarkDeleted(DateTimeOffset at, Guid by)
    {
        IsDeleted = true;
        DeletedAt = at;
        DeletedBy = by;
    }
}
