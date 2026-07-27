namespace FusionOS.Modules.Crm.Application.Leads.Services;

/// <summary>
/// Phase 4 CRM — lead scoring. This is a small, honestly-scoped, deterministic
/// RULES-BASED calculation, not machine learning or any kind of "AI" black box: it only
/// looks at signals already visible to the callers that use it — does this lead have
/// attribution (Source), a known company (AccountId), and at least one logged Activity.
/// Each signal is a flat point value with no weighting model behind it; the numbers are
/// chosen to be legible to a human reading this file, not tuned against data.
///
/// The three signals below sum to at most 70 of the nominal 0-100 range on purpose — that
/// headroom is left for future signals (e.g. opportunity value, email engagement) to be
/// added later without needing to renormalize scores that already exist on real leads.
///
/// Used by CreateLeadCommandHandler (initial score, computed from whatever is known at
/// creation time) and by RecalculateLeadScoreCommandHandler (a full refresh once the lead
/// may have gained an Account and/or logged Activities).
/// </summary>
public static class LeadScoringService
{
    public const int SourceAttributionPoints = 20;
    public const int AccountAssignedPoints = 30;
    public const int HasLoggedActivityPoints = 20;

    /// <summary>
    /// hasLoggedActivity is passed in rather than looked up here — this service stays a
    /// pure, dependency-free calculation; whichever caller has (or is willing to fetch)
    /// an IActivityRepository decides how to answer that question.
    /// </summary>
    public static int Calculate(string? source, Guid? accountId, bool hasLoggedActivity)
    {
        var score = 0;

        if (!string.IsNullOrWhiteSpace(source))
            score += SourceAttributionPoints;

        if (accountId is not null)
            score += AccountAssignedPoints;

        if (hasLoggedActivity)
            score += HasLoggedActivityPoints;

        return score;
    }
}
