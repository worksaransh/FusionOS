using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.CreateQualityAudit;

public sealed class CreateQualityAuditValidator : AbstractValidator<CreateQualityAuditCommand>
{
    public CreateQualityAuditValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AuditType).IsInEnum();
        RuleFor(x => x.ScheduledDate).NotEmpty();
        RuleFor(x => x.Auditor).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Scope).NotEmpty().MaximumLength(2000);
    }
}
