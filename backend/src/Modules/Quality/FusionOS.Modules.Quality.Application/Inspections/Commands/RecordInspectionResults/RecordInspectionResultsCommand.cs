using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.RecordInspectionResults;

/// <summary>
/// RaiseHoldOnFailure/HeldEntityType are an explicit, opt-in escalation, never an automatic
/// side effect — same restraint InspectionCompleted's own doc comment already documents
/// (quarantining stock reaches into another module's data with no consumer wired to react to
/// it yet). Default is false/null, so every existing caller keeps recording results exactly
/// as before with zero downstream effect. When the caller sets RaiseHoldOnFailure=true AND
/// the inspection resolves to Failed, a QualityHold is created with EntityType=HeldEntityType
/// and EntityId=this inspection's own ReferenceId, InspectionId set to this inspection.
/// HeldEntityType is required only when RaiseHoldOnFailure is true (validated).
/// </summary>
public sealed record RecordInspectionResultsCommand(
    Guid CompanyId,
    Guid InspectionId,
    IReadOnlyList<InspectionResultInput> Results,
    bool RaiseHoldOnFailure = false,
    string? HeldEntityType = null)
    : ICommand<InspectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.inspection.record" };
    public string EntityType => nameof(Domain.Inspections.Inspection);
    public Guid EntityId => InspectionId;
    public string Action => "ResultsRecorded";
}
