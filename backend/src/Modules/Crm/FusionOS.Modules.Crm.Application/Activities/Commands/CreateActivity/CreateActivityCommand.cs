using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Activities.Contracts;

namespace FusionOS.Modules.Crm.Application.Activities.Commands.CreateActivity;

/// <summary>
/// Logs an interaction against some (EntityType, EntityId) pair — same
/// EntityType/EntityId-doubles-as-the-IAuditableCommand-identity convention as
/// Core's CreateApprovalRequestCommand: the audit trail for "an activity was
/// logged" is naturally about the same target the activity itself is against,
/// so these constructor parameters satisfy IAuditableCommand directly with no
/// separate declaration needed. Type travels as a string (validated by
/// CreateActivityValidator, parsed in the handler) — same enum-over-the-wire
/// convention as Finance's CreateAccountCommand.AccountType.
/// </summary>
public sealed record CreateActivityCommand(Guid CompanyId, string EntityType, Guid EntityId, string Type, string Notes)
    : ICommand<ActivityDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.activity.create" };
    public string Action => "Created";
}
