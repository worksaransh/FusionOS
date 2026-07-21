using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Contacts.Commands.DeactivateContact;

public sealed class DeactivateContactValidator : AbstractValidator<DeactivateContactCommand>
{
    public DeactivateContactValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.ContactId).NotEmpty();
    }
}
