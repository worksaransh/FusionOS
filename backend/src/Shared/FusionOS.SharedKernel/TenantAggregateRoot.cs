using FusionOS.SharedKernel.Auditing;

namespace FusionOS.SharedKernel;

/// <summary>
/// Base for the overwhelming majority of FusionOS aggregate roots: audited,
/// soft-deletable, optimistically concurrent, and scoped to a Company/Branch.
/// </summary>
public abstract class TenantAggregateRoot : AggregateRoot, IAuditable, ITenantScoped, ISoftDeletable, IHasRowVersion
{
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }
    public Guid CompanyId { get; protected set; }
    public Guid? BranchId { get; protected set; }
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
