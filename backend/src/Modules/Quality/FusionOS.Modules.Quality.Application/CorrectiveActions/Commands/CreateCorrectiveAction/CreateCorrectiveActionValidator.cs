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
    }
}
