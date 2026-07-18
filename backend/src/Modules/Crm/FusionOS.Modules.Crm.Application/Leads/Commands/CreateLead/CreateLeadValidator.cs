using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.CreateLead;

public sealed class CreateLeadValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ContactEmail).MaximumLength(320);
        RuleFor(x => x.ContactPhone).MaximumLength(50);
        RuleFor(x => x.Source).MaximumLength(100);
    }
}
