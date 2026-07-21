using FluentValidation;

namespace FusionOS.Modules.Warehouse.Application.Packages.Commands.CreatePackage;

public sealed class CreatePackageValidator : AbstractValidator<CreatePackageCommand>
{
    public CreatePackageValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.PickListId).NotEmpty();
        RuleFor(x => x.PackageNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.WeightKg).GreaterThanOrEqualTo(0m).When(x => x.WeightKg.HasValue);
        RuleFor(x => x.LengthCm).GreaterThanOrEqualTo(0m).When(x => x.LengthCm.HasValue);
        RuleFor(x => x.WidthCm).GreaterThanOrEqualTo(0m).When(x => x.WidthCm.HasValue);
        RuleFor(x => x.HeightCm).GreaterThanOrEqualTo(0m).When(x => x.HeightCm.HasValue);
        RuleFor(x => x.Lines).NotEmpty().WithMessage("A package must have at least one line.");
        RuleForEach(x => x.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.ProductId).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
        });
    }
}
