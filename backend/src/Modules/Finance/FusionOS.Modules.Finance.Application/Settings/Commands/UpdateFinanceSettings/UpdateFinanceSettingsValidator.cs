using FluentValidation;

namespace FusionOS.Modules.Finance.Application.Settings.Commands.UpdateFinanceSettings;

public sealed class UpdateFinanceSettingsValidator : AbstractValidator<UpdateFinanceSettingsCommand>
{
    public UpdateFinanceSettingsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}
