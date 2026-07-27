using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Campaigns.Commands.DeactivateCampaign;

public sealed class DeactivateCampaignValidator : AbstractValidator<DeactivateCampaignCommand>
{
    public DeactivateCampaignValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CampaignId).NotEmpty();
    }
}
