using FluentValidation;

namespace FusionOS.Modules.Billing.Application.Subscriptions.Commands.CreateSubscription;

public sealed class CreateSubscriptionValidator : AbstractValidator<CreateSubscriptionCommand>
{
    public CreateSubscriptionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PlanId).NotEmpty();
        RuleFor(x => x.TrialDays).GreaterThan(0).When(x => x.TrialDays.HasValue);
    }
}
