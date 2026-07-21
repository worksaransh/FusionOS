using FluentValidation;

namespace FusionOS.Modules.Quality.Application.NonConformanceReports.Commands.UpdateNonConformanceReportStatus;

public sealed class UpdateNonConformanceReportStatusValidator : AbstractValidator<UpdateNonConformanceReportStatusCommand>
{
    public UpdateNonConformanceReportStatusValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.NonConformanceReportId).NotEmpty();
        RuleFor(x => x.Status).IsInEnum();
    }
}
