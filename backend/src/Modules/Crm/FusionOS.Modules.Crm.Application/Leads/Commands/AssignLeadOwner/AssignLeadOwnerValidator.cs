using FluentValidation;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadOwner;

public sealed class AssignLeadOwnerValidator : AbstractValidator<AssignLeadOwnerCommand>
{
    public AssignLeadOwnerValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.LeadId).NotEmpty();
    }
}
