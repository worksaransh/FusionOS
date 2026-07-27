namespace FusionOS.Modules.Crm.Domain.Opportunities;

/// <summary>
/// Stored as text via EF value conversion, never a native PostgreSQL enum — 04_DATABASE_GUIDELINES.md §10.
///
/// <see cref="Open"/> is kept (not renamed/removed) as the legacy default initial stage —
/// existing rows, the OpportunityTests suite (asserts <c>Stage == Open</c> immediately after
/// <c>Create</c>), and every handler that gated Win/Lose on "stage == Open" all depend on it
/// still existing and still being what <c>Create</c> assigns. Prospecting/Qualification/
/// Proposal/Negotiation are purely additive intermediate pipeline stages reached afterwards
/// via <see cref="Opportunity.ChangeStage"/>. Win/Lose were relaxed from "must be exactly
/// Open" to "must not already be Won/Lost" so a deal can close from any of these stages, not
/// just from Open.
/// </summary>
public enum OpportunityStage
{
    Open,
    Prospecting,
    Qualification,
    Proposal,
    Negotiation,
    Won,
    Lost,
}
