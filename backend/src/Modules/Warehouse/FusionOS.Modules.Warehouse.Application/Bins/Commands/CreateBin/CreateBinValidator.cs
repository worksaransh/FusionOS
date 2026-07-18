using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Bins.Commands.CreateBin;

public sealed class CreateBinValidator : AbstractValidator<CreateBinCommand>
{
    public CreateBinValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ZoneId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(20);
    }
}
