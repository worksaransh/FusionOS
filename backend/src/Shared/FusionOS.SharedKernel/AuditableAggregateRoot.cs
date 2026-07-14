using FusionOS.SharedKernel.Auditing;

namespace FusionOS.SharedKernel;

/// <summary>
/// Base for aggregate roots that are audited/soft-deletable but NOT tenant-scoped
/// (e.g. Company, the tenant root itself). Combines AggregateRoot's domain-event
/// support with the standard audit columns from 04_DATABASE_GUIDELINES.md §3.
/// </summary>
public abstract class AuditableAggregateRoot : AggregateRoot, IAuditable, ISoftDeletable, IHasRowVersion
{
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public Guid? DeletedBy { get; private set; }
    public byte[]? RowVersion { get; private set; }

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
