using FluentValidation;

namespace FusionOS.Modules.Core.Application.Branches.Commands.DeactivateBranch;

public sealed class DeactivateBranchValidator : AbstractValidator<DeactivateBranchCommand>
{
    public DeactivateBranchValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
    }
}
