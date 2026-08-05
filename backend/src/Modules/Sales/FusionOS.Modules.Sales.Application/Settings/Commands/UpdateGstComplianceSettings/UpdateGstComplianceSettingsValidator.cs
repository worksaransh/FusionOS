using FluentValidation;

namespace FusionOS.Modules.Sales.Application.Settings.Commands.UpdateGstComplianceSettings;

public sealed class UpdateGstComplianceSettingsValidator : AbstractValidator<UpdateGstComplianceSettingsCommand>
{
    public UpdateGstComplianceSettingsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}
