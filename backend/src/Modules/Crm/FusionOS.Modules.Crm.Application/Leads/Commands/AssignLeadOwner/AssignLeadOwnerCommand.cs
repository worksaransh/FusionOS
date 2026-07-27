using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Leads.Contracts;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadOwner;

/// <summary>
/// Sets or clears (pass null) which Core user owns/works this lead — "territory via
/// ownership". Scope note: this is a manual, single-field ownership assignment only.
/// A full territory RULE engine (auto-assignment by region/industry/round-robin, with
/// its own rule aggregate and evaluation pipeline) is a real, separate follow-up, not
/// something this slice reaches for — same "honest scoping, not a fabricated feature"
/// discipline as Finance's GetTaxPeriodSummaryReportQuery. OwnerId is never
/// existence-validated against Core's User table here — same opaque cross-module
/// reference convention as MaintenanceRequest.AssignTechnician.
/// </summary>
public sealed record AssignLeadOwnerCommand(Guid CompanyId, Guid LeadId, Guid? OwnerId)
    : ICommand<LeadDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.lead.assign-owner" };
    public string EntityType => nameof(Domain.Leads.Lead);
    public Guid EntityId => LeadId;
    public string Action => "OwnerAssigned";
}
