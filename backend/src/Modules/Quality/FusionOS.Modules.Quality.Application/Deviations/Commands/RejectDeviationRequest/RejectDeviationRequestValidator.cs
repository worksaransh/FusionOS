using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.RejectDeviationRequest;

public sealed class RejectDeviationRequestValidator : AbstractValidator<RejectDeviationRequestCommand>
{
    public RejectDeviationRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DeviationRequestId).NotEmpty();
        RuleFor(x => x.DecidedByUserId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(2000);
    }
}
