using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Domain.Inspections;

namespace FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;

/// <summary>
/// InspectionPlanId is optional and purely a convenience default: when supplied and
/// Characteristics is empty/omitted, the handler loads that InspectionPlan and copies its
/// characteristic names in. If the caller ALSO supplies explicit Characteristics, those win —
/// the plan never overrides an explicit list, so every existing caller that has never heard
/// of an InspectionPlan keeps working completely unchanged.
///
/// <see cref="ControlLimits"/> is likewise optional and additive (added for the SPC slice):
/// when supplied, it carries SPC control limits for characteristics named in
/// <see cref="Characteristics"/> (matched by name, case-insensitive; a characteristic with no
/// matching entry — or when ControlLimits is null entirely — is created as plain pass/fail,
/// with both limits null). Kept as a separate trailing optional list rather than folding into
/// Characteristics itself so every existing caller passing a plain string list keeps
/// compiling unchanged.
/// </summary>
public sealed record CreateInspectionCommand(
    Guid CompanyId,
    InspectionType Type,
    Guid ReferenceId,
    IReadOnlyList<string> Characteristics,
    Guid? InspectionPlanId = null,
    IReadOnlyList<InspectionCharacteristicControlLimits>? ControlLimits = null)
    : ICommand<InspectionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "quality.inspection.create" };
    public string EntityType => nameof(Domain.Inspections.Inspection);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}

/// <summary>One characteristic's optional SPC control limits, matched to <see cref="CreateInspectionCommand.Characteristics"/> by name.</summary>
public sealed record InspectionCharacteristicControlLimits(string Characteristic, decimal? LowerControlLimit, decimal? UpperControlLimit);
