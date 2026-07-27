using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Deviations.Commands.CreateDeviationRequest;

public sealed class CreateDeviationRequestValidator : AbstractValidator<CreateDeviationRequestCommand>
{
    public CreateDeviationRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.InspectionId).NotEqual(Guid.Empty).When(x => x.InspectionId.HasValue);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.RequestedByUserId).NotEmpty();
    }
}
