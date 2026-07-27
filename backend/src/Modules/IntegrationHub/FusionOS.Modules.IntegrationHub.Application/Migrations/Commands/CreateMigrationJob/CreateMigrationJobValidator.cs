using FluentValidation;

namespace FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.CreateMigrationJob;

public sealed class CreateMigrationJobValidator : AbstractValidator<CreateMigrationJobCommand>
{
    public CreateMigrationJobValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SourceSystem).NotEmpty().MaximumLength(100);
    }
}
