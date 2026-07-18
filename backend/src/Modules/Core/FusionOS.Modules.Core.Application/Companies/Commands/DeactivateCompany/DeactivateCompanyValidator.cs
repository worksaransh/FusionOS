using FluentValidation;

namespace FusionOS.Modules.Core.Application.Companies.Commands.DeactivateCompany;

public sealed class DeactivateCompanyValidator : AbstractValidator<DeactivateCompanyCommand>
{
    public DeactivateCompanyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
