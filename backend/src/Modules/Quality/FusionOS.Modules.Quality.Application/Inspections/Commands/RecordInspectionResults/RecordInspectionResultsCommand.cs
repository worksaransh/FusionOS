using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.RecordInspectionResults;

public sealed record RecordInspectionResultsCommand(Guid CompanyId, Guid InspectionId, IReadOnlyList<InspectionResultInput> Results)
    : ICommand<InspectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.inspection.record" };
    public string EntityType => nameof(Domain.Inspections.Inspection);
    public Guid EntityId => InspectionId;
    public string Action => "ResultsRecorded";
}
