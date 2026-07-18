using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Contracts;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Commands.RecordKpiSnapshot;

public sealed record RecordKpiSnapshotCommand(Guid CompanyId, Guid KpiDefinitionId, decimal Value, string? Notes)
    : ICommand<KpiSnapshotDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "bi.kpi-snapshot.record" };
    public string EntityType => nameof(Domain.KpiSnapshots.KpiSnapshot);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
