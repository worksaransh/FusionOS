namespace FusionOS.Modules.Quality.Domain.Inspections;

/// <summary>Input shape for recording one characteristic's result on an inspection — kept in Domain so both Application and tests reference one definition.</summary>
public sealed record InspectionResultInput(string Characteristic, bool Passed, string? Notes);
