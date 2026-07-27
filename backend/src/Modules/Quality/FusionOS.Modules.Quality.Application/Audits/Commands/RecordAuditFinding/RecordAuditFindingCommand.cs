using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Audits.Contracts;
using FusionOS.Modules.Quality.Domain.Audits;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.RecordAuditFinding;

public sealed record RecordAuditFindingCommand(Guid CompanyId, Guid QualityAuditId, string Description, AuditFindingSeverity Severity)
    : ICommand<QualityAuditDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.audit.record-finding" };
    public string EntityType => nameof(Domain.Audits.QualityAudit);
    public Guid EntityId => QualityAuditId;
    public string Action => "FindingRecorded";
}
