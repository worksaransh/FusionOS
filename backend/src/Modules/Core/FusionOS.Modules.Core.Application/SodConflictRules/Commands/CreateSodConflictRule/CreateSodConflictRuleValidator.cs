using FluentValidation;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Commands.CreateSodConflictRule;

public sealed class CreateSodConflictRuleValidator : AbstractValidator<CreateSodConflictRuleCommand>
{
    public CreateSodConflictRuleValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ConflictingPermissionA).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ConflictingPermissionB).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(500);
        RuleFor(x => x)
            .Must(x => !string.Equals(x.ConflictingPermissionA?.Trim(), x.ConflictingPermissionB?.Trim(), StringComparison.OrdinalIgnoreCase))
            .WithMessage("A conflict rule must name two different permission codes.")
            .WithName(nameof(CreateSodConflictRuleCommand.ConflictingPermissionB));
    }
}
