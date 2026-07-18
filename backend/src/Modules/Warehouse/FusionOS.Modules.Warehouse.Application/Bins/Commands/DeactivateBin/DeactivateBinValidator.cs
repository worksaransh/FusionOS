using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.DeactivateBin;

public sealed class DeactivateBinValidator : AbstractValidator<DeactivateBinCommand>
{
    public DeactivateBinValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
    }
}
