using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Audits.Commands.RecordAuditFinding;

public sealed class RecordAuditFindingValidator : AbstractValidator<RecordAuditFindingCommand>
{
    public RecordAuditFindingValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.QualityAuditId).NotEmpty();
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Severity).IsInEnum();
    }
}
