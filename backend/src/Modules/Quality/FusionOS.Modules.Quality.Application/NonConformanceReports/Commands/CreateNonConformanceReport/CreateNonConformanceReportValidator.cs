using FluentValidation;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.CreateNonConformanceReport;

public sealed class CreateNonConformanceReportValidator : AbstractValidator<CreateNonConformanceReportCommand>
{
    public CreateNonConformanceReportValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.InspectionId).NotEqual(Guid.Empty).When(x => x.InspectionId.HasValue);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(2000);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.RaisedByUserId).NotEmpty();
    }
}
