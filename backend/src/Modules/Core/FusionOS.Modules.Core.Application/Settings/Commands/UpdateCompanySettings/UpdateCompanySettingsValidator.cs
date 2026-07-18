using FluentValidation;

namespace FusionOS.Modules.Core.Application.Settings.Commands.UpdateCompanySettings;

public sealed class UpdateCompanySettingsValidator : AbstractValidator<UpdateCompanySettingsCommand>
{
    public UpdateCompanySettingsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.DefaultCurrency).NotEmpty().Length(3).WithMessage("Default currency must be a 3-letter ISO 4217 code.");
        RuleFor(x => x.DefaultPageSize).InclusiveBetween(1, 200);
        RuleFor(x => x.DisplayName).MaximumLength(200);
        RuleFor(x => x.LogoUrl).MaximumLength(2000);
    }
}
