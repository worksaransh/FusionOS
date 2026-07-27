using FluentValidation;

namespace FusionOS.Modules.Quality.Application.CorrectiveActions.Commands.CreateCorrectiveAction;

public sealed class CreateCorrectiveActionValidator : AbstractValidator<CreateCorrectiveActionCommand>
{
    public CreateCorrectiveActionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.NonConformanceReportId).NotEmpty();
        RuleFor(x => x.RootCauseDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.CorrectiveActionDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.PreventiveActionDescription).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.AssignedToUserId).NotEmpty();
        RuleFor(x => x.DueDate).NotEmpty();
        RuleFor(x => x.RcaMethod).IsInEnum();
        RuleFor(x => x.Why1).MaximumLength(500);
        RuleFor(x => x.Why2).MaximumLength(500);
        RuleFor(x => x.Why3).MaximumLength(500);
        RuleFor(x => x.Why4).MaximumLength(500);
        RuleFor(x => x.Why5).MaximumLength(500);
    }
}
