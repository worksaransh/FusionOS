using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.UpdateBin;

public sealed class UpdateBinValidator : AbstractValidator<UpdateBinCommand>
{
    public UpdateBinValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
    }
}
