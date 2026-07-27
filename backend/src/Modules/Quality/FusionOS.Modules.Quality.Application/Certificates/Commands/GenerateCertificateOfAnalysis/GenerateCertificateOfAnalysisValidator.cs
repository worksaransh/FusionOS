using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Certificates.Commands.GenerateCertificateOfAnalysis;

public sealed class GenerateCertificateOfAnalysisValidator : AbstractValidator<GenerateCertificateOfAnalysisCommand>
{
    public GenerateCertificateOfAnalysisValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.InspectionId).NotEmpty();
        RuleFor(x => x.IssuedByUserId).NotEmpty();
    }
}
