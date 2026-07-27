namespace FusionOS.Modules.Quality.Domain.CorrectiveActions;

/// <summary>
/// The root-cause-analysis technique used to arrive at <see cref="CorrectiveAction.RootCauseDescription"/>.
/// Additive alongside that pre-existing free-text field — this only tags which method was
/// used and, for FiveWhys, optionally captures the structured chain (see
/// <see cref="CorrectiveAction.Why1"/>-<see cref="CorrectiveAction.Why5"/>). Other is the
/// default so every pre-existing row (and every caller that doesn't set this explicitly)
/// keeps working unchanged. Stored as text via EF value conversion, never a native
/// PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.
/// </summary>
public enum RcaMethod
{
    Other,
    FiveWhys,
    Fishbone,
    Pareto,
}
