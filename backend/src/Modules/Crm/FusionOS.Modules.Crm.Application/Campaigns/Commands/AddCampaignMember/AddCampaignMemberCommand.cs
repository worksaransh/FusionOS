using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Crm.Application.Campaigns.Contracts;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.AddCampaignMember;

/// <summary>
/// Enrolls a Lead or Contact (MemberEntityType/MemberEntityId) into a Campaign.
/// MemberEntityType is restricted to "Lead"/"Contact" by the validator — the only two
/// entity kinds this slice enrolls. Named MemberEntityType/MemberEntityId (rather than
/// EntityType/EntityId) to avoid colliding with IAuditableCommand's own EntityType/
/// EntityId, which here identify the audited CampaignMembership record itself.
/// </summary>
public sealed record AddCampaignMemberCommand(Guid CompanyId, Guid CampaignId, string MemberEntityType, Guid MemberEntityId)
    : ICommand<CampaignMembershipDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.campaign.manage-membership" };
    public string EntityType => nameof(Domain.Campaigns.CampaignMembership);
    public Guid EntityId => CampaignId;
    public string Action => "MemberAdded";
}
