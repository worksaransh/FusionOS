using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.RemoveCampaignMember;

public sealed class RemoveCampaignMemberValidator : AbstractValidator<RemoveCampaignMemberCommand>
{
    public RemoveCampaignMemberValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CampaignId).NotEmpty();
        RuleFor(x => x.MemberEntityId).NotEmpty();
        RuleFor(x => x.MemberEntityType).NotEmpty();
    }
}
