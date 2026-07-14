namespace FusionOS.SharedKernel.Auditing;

/// <summary>Hard deletes are forbidden platform-wide — 04_DATABASE_GUIDELINES.md §4.</summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTimeOffset? DeletedAt { get; }
    Guid? DeletedBy { get; }
}
