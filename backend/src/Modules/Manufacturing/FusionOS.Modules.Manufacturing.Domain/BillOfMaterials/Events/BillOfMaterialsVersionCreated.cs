using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.Events;

/// <summary>Raised when a new BOM version supersedes a previous one (Phase 6, 2026-07-22). No consumer today, same "future hook" pattern as BillOfMaterialsCreated.</summary>
public sealed record BillOfMaterialsVersionCreated(Guid BillOfMaterialsId, Guid CompanyId, Guid ProductId, Guid PreviousVersionId, int Version) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
