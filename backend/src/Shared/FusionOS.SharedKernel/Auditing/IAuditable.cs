namespace FusionOS.SharedKernel.Auditing;

/// <summary>Mandatory audit columns per 04_DATABASE_GUIDELINES.md §3.</summary>
public interface IAuditable
{
    DateTimeOffset CreatedAt { get; }
    Guid CreatedBy { get; }
    DateTimeOffset? UpdatedAt { get; }
    Guid? UpdatedBy { get; }
}
