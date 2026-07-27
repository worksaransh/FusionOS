using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.LoadMigrationStagingRecords;

public sealed class LoadMigrationStagingRecordsValidator : AbstractValidator<LoadMigrationStagingRecordsCommand>
{
    public LoadMigrationStagingRecordsValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MigrationJobId).NotEmpty();
    }
}
