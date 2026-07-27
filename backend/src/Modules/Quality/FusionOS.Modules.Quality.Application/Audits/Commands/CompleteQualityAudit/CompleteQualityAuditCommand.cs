using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Audits.Contracts;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.CompleteQualityAudit;

public sealed record CompleteQualityAuditCommand(Guid CompanyId, Guid QualityAuditId)
    : ICommand<QualityAuditDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.audit.create" };
    public string EntityType => nameof(Domain.Audits.QualityAudit);
    public Guid EntityId => QualityAuditId;
    public string Action => "Completed";
}
