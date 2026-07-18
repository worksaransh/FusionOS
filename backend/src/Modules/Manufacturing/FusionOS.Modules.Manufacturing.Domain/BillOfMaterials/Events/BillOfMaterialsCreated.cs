using FusionOS.SharedKernel;

namespace FusionOS.Modules.Manufacturing.Domain.BillOfMaterials.Events;

/// <summary>Raised when a bill of materials is defined. No consumer today — a natural future hook for MRP/demand planning.</summary>
public sealed record BillOfMaterialsCreated(Guid BillOfMaterialsId, Guid CompanyId, Guid ProductId, string Code) : IDomainEvent
{
    public DateTimeOffset OccurredOn { get; } = DateTimeOffset.UtcNow;
}
