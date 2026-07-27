using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Audits.Contracts;
using FusionOS.Modules.Quality.Domain.Audits;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.CreateQualityAudit;

/// <summary>
/// Permission split (documented per the task's own "your call"): quality.audit.create covers the
/// whole create -> start -> complete lifecycle, same single-permission-per-aggregate-lifecycle
/// convention CorrectiveAction uses distinct permissions for (create/start/close/verify) — but
/// unlike CorrectiveAction's four-way split, Start/Complete here are trivial status flips with no
/// domain-specific side effects of their own, so one permission covering the lifecycle avoids
/// forcing a caller to be granted four near-identical permissions for what is really one workflow.
/// </summary>
public sealed record CreateQualityAuditCommand(
    Guid CompanyId,
    QualityAuditType AuditType,
    DateTimeOffset ScheduledDate,
    string Auditor,
    string Scope)
    : ICommand<QualityAuditDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.audit.create" };
    public string EntityType => nameof(Domain.Audits.QualityAudit);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
