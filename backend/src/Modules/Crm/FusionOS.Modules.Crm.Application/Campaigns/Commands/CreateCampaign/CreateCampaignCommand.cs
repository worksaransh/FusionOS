using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.CreateCampaign;

public sealed record CreateCampaignCommand(Guid CompanyId, string Name, string? Description, DateTimeOffset StartDate, DateTimeOffset? EndDate)
    : ICommand<CampaignDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.campaign.create" };
    public string EntityType => nameof(Domain.Campaigns.Campaign);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
