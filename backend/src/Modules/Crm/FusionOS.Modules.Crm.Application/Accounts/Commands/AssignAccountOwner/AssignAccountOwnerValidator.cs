using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Accounts.Commands.AssignAccountOwner;

public sealed class AssignAccountOwnerValidator : AbstractValidator<AssignAccountOwnerCommand>
{
    public AssignAccountOwnerValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
    }
}
