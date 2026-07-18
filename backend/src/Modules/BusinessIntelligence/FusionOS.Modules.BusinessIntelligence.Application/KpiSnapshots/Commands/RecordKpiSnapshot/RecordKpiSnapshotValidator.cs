using FluentValidation;

namespace FusionOS.Modules.BusinessIntelligence.Application.KpiSnapshots.Commands.RecordKpiSnapshot;

public sealed class RecordKpiSnapshotValidator : AbstractValidator<RecordKpiSnapshotCommand>
{
    public RecordKpiSnapshotValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.KpiDefinitionId).NotEmpty();
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}
