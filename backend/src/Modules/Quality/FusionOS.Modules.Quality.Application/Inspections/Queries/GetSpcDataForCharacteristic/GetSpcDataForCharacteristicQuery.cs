using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;

namespace FusionOS.Modules.Quality.Application.Inspections.Queries.GetSpcDataForCharacteristic;

/// <summary>
/// Statistical Process Control (SPC) raw data for one characteristic name, across every
/// inspection in the optional period: every recorded MeasuredValue plus the most-recently-set
/// control limits found for it.
///
/// <b>Deliberately scoped down, following the same honest-gap style as Finance's
/// GetTaxPeriodSummaryReportQuery:</b> there is no per-characteristic-spec master record in
/// this codebase — control limits are set per <see cref="Domain.Inspections.InspectionItem"/>,
/// at inspection-creation time, not on a single canonical "characteristic spec" shared across
/// every inspection that checks it. Two different inspections could (today, with nothing
/// stopping them) set different limits for a characteristic of the same name. This query does
/// NOT attempt to reconcile that: it returns the limits from whichever matching item was most
/// recently created (by the owning Inspection's CreatedAt), and documents that as the honest
/// scope-out here — a real SPC feature would need a proper per-characteristic-spec master
/// record (e.g. an InspectionPlan characteristic carrying the canonical limits) that every
/// inspection checking that characteristic defers to, rather than each inspection carrying
/// its own copy. That master record is a real, separately-scoped follow-up.
///
/// This also does NOT compute Cp/Cpk process-capability indices — that is a second, explicit
/// scope-out: this returns only the raw data points (measured value + timestamp) and the
/// limits, so a caller/frontend can chart a control chart itself. Computing capability indices
/// correctly requires decisions (sample subgrouping, distribution assumptions) this slice does
/// not make.
///
/// RecordedAt on each data point is the owning Inspection's CreatedAt (audit timestamp) — the
/// only timestamp available, since InspectionItem itself carries no timestamp of its own
/// (it's a child line owned entirely by its parent Inspection, same as every other line entity
/// in this codebase).
///
/// Permission: reuses quality.inspection.read rather than a new code — this is just existing
/// inspection data (MeasuredValue/control limits already visible via GetInspectionByIdQuery)
/// reshaped for charting, not a new category of data.
/// </summary>
public sealed record GetSpcDataForCharacteristicQuery(Guid CompanyId, string CharacteristicName, DateTimeOffset? PeriodStart, DateTimeOffset? PeriodEnd)
    : IQuery<SpcDataForCharacteristicDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "quality.inspection.read" };
}
