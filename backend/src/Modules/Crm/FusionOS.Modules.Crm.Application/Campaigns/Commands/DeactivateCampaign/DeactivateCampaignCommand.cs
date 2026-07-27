using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.DeactivateCampaign;

/// <summary>
/// Soft-deactivate only — see Campaign.Deactivate(). Never a hard delete. No dedicated
/// "crm.campaign.deactivate" permission is reserved in PermissionCatalog — only
/// crm.campaign.create/read/manage-membership are — so this reuses
/// crm.campaign.create, the other campaign-lifecycle-write permission, rather than
/// crm.campaign.manage-membership (which is scoped to who's enrolled, not the
/// campaign's own lifecycle). If a dedicated deactivate permission is wanted later,
/// it needs to be added to PermissionCatalog first.
/// </summary>
public sealed record DeactivateCampaignCommand(Guid CompanyId, Guid CampaignId)
    : ICommand<CampaignDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.campaign.create" };
    public string EntityType => nameof(Domain.Campaigns.Campaign);
    public Guid EntityId => CampaignId;
    public string Action => "Deactivated";
}
