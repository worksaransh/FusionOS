using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.AddCampaignMember;

public sealed class AddCampaignMemberValidator : AbstractValidator<AddCampaignMemberCommand>
{
    private static readonly string[] AllowedEntityTypes = { "Lead", "Contact" };

    public AddCampaignMemberValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CampaignId).NotEmpty();
        RuleFor(x => x.MemberEntityId).NotEmpty();
        RuleFor(x => x.MemberEntityType)
            .NotEmpty()
            .Must(entityType => AllowedEntityTypes.Contains(entityType))
            .WithMessage($"Entity type must be one of: {string.Join(", ", AllowedEntityTypes)}.");
    }
}
