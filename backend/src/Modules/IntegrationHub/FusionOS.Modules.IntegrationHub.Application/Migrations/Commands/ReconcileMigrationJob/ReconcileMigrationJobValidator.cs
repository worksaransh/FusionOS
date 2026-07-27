using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ReconcileMigrationJob;

public sealed class ReconcileMigrationJobValidator : AbstractValidator<ReconcileMigrationJobCommand>
{
    public ReconcileMigrationJobValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.MigrationJobId).NotEmpty();
    }
}
