namespace FusionOS.SharedKernel;

/// <summary>
/// Marker for in-process domain events (03_SYSTEM_ARCHITECTURE.md §4.1).
/// Dispatched synchronously within the same unit of work, never across module boundaries.
/// </summary>
public interface IDomainEvent
{
    DateTimeOffset OccurredOn { get; }
}
