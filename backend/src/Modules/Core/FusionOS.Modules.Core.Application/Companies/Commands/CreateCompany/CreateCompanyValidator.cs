using FluentValidation;

namespace FusionOS.Modules.Core.Application.Companies.Commands.CreateCompany;

public sealed class CreateCompanyValidator : AbstractValidator<CreateCompanyCommand>
{
    public CreateCompanyValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.LegalName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.BaseCurrency).NotEmpty().Length(3);
    }
}
