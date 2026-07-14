namespace FusionOS.SharedKernel.Auditing;

/// <summary>Optimistic concurrency token — 04_DATABASE_GUIDELINES.md §8.</summary>
public interface IHasRowVersion
{
    byte[]? RowVersion { get; }
}
