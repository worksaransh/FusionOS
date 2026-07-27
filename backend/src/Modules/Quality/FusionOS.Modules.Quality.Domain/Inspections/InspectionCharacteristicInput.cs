namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>
/// Input shape for one characteristic at inspection-creation time, richer than the plain
/// characteristic-name overload of <see cref="Inspection.Create(Guid, InspectionType, Guid, IReadOnlyCollection{string})"/>:
/// carries the optional SPC control limits a characteristic can be given up front (both
/// null for a pass/fail-only characteristic — the common case).
/// </summary>
public sealed record InspectionCharacteristicInput(string Characteristic, decimal? LowerControlLimit = null, decimal? UpperControlLimit = null);
