using FusionOS.BuildingBlocks.Application.Abstractions;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.RemoveCampaignMember;

/// <summary>Removes a previously-added (MemberEntityType, MemberEntityId) enrollment from a Campaign — a hard delete of the join row, same as removing a row from any join table (see CampaignMembership's doc comment for why this one genuinely allows a hard delete unlike Activity).</summary>
public sealed record RemoveCampaignMemberCommand(Guid CompanyId, Guid CampaignId, string MemberEntityType, Guid MemberEntityId)
    : ICommand, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "crm.campaign.manage-membership" };
    public string EntityType => nameof(Domain.Campaigns.CampaignMembership);
    public Guid EntityId => CampaignId;
    public string Action => "MemberRemoved";
}
