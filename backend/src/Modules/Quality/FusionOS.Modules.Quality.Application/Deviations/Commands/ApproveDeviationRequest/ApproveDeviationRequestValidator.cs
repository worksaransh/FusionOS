using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.ApproveDeviationRequest;

public sealed class ApproveDeviationRequestValidator : AbstractValidator<ApproveDeviationRequestCommand>
{
    public ApproveDeviationRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DeviationRequestId).NotEmpty();
        RuleFor(x => x.DecidedByUserId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
