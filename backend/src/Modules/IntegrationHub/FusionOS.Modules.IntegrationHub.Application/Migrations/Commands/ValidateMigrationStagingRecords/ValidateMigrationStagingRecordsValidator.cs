using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ValidateMigrationStagingRecords;

public sealed class ValidateMigrationStagingRecordsValidator : AbstractValidator<ValidateMigrationStagingRecordsCommand>
{
    public ValidateMigrationStagingRecordsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MigrationJobId).NotEmpty();
    }
}
