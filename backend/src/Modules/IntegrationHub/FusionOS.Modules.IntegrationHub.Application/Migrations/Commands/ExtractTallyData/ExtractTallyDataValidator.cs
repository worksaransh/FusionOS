using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ExtractTallyData;

public sealed class ExtractTallyDataValidator : AbstractValidator<ExtractTallyDataCommand>
{
    public ExtractTallyDataValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MigrationJobId).NotEmpty();
        RuleFor(x => x.TallyXmlContent).NotEmpty();
    }
}
