using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadAccount;

/// <summary>Sets or clears (pass null) which Account this lead's organization corresponds to — same shape as Sales' AssignPriceListCommand.</summary>
public sealed record AssignLeadAccountCommand(Guid CompanyId, Guid LeadId, Guid? AccountId)
    : ICommand<LeadDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.lead.assign-account" };
    public string EntityType => nameof(Domain.Leads.Lead);
    public Guid EntityId => LeadId;
    public string Action => "AccountAssigned";
}
