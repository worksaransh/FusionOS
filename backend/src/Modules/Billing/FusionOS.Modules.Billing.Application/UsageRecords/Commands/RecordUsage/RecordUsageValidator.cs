using FluentValidation;

namespace FusionOS.Modules.Billing.Application.UsageRecords.Commands.RecordUsage;

public sealed class RecordUsageValidator : AbstractValidator<RecordUsageCommand>
{
    public RecordUsageValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.MetricType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0);
    }
}
