using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.AssignAccountOwner;

/// <summary>
/// Sets or clears (pass null) which Core user owns this CRM account — "territory via
/// ownership". Same manual-assignment-only scope as AssignLeadOwnerCommand (see its
/// doc comment) — no territory rule engine here. OwnerId is never existence-validated
/// against Core's User table, same opaque cross-module reference convention as
/// MaintenanceRequest.AssignTechnician.
/// </summary>
public sealed record AssignAccountOwnerCommand(Guid CompanyId, Guid AccountId, Guid? OwnerId)
    : ICommand<AccountDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.account.assign-owner" };
    public string EntityType => nameof(Domain.Accounts.Account);
    public Guid EntityId => AccountId;
    public string Action => "OwnerAssigned";
}
