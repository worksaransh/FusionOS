using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.Events;

/// <summary>Raised when a routing operation is appended to a bill of materials. No consumer today — same "no consumer yet" pattern as BillOfMaterialsCreated.</summary>
public sealed record RoutingOperationAdded(Guid BillOfMaterialsId, Guid CompanyId, Guid OperationId, int SequenceNumber, string OperationName) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

/// <summary>Raised when a routing operation is removed from a bill of materials.</summary>
public sealed record RoutingOperationRemoved(Guid BillOfMaterialsId, Guid CompanyId, Guid OperationId) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}

/// <summary>Raised when a bill of materials' whole routing is reordered — carries the new, complete sequence of operation ids.</summary>
public sealed record RoutingOperationsReordered(Guid BillOfMaterialsId, Guid CompanyId, IReadOnlyList<Guid> OrderedOperationIds) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
