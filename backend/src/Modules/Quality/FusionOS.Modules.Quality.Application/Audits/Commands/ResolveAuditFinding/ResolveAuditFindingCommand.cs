using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Audits.Contracts;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.ResolveAuditFinding;

public sealed record ResolveAuditFindingCommand(Guid CompanyId, Guid QualityAuditId, Guid FindingId)
    : ICommand<QualityAuditDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.audit.record-finding" };
    public string EntityType => nameof(Domain.Audits.QualityAudit);
    public Guid EntityId => QualityAuditId;
    public string Action => "FindingResolved";
}
