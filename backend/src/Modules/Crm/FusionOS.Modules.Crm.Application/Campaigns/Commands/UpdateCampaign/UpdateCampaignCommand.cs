using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.UpdateCampaign;

public sealed record UpdateCampaignCommand(Guid CompanyId, Guid CampaignId, string Name, string? Description, DateTimeOffset StartDate, DateTimeOffset? EndDate)
    : ICommand<CampaignDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.campaign.update" };
    public string EntityType => nameof(Domain.Campaigns.Campaign);
    public Guid EntityId => CampaignId;
    public string Action => "Updated";
}
