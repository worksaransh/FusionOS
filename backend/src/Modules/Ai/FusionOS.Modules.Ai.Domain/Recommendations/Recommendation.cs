using FusionOS.SharedKernel;
using FusionOS.Modules.Ai.Domain.Recommendations.Events;

namespace FusionOS.Modules.Ai.Domain.Recommendations;

/// <summary>
/// Phase 7 — AI Platform, first slice. `docs/blueprint/12_AI_PLATFORM.md` §3
/// describes the AI module as a ".NET AI orchestration layer" that "receives
/// AI-produced recommendations/insights as events" and exposes them for
/// human confirmation ("recommendation-class outputs... require explicit
/// user confirmation before they affect the transactional ledger", §5) —
/// this aggregate IS that orchestration-layer record: a durable,
/// audit-tracked recommendation, Pending → Accepted/Dismissed.
///
/// Deliberately does NOT include any real forecasting/OCR/ML model — the
/// Python ML services §3.1 describes (time-series forecasting, OCR
/// pipelines) are a separate workstream requiring real training data and
/// external infrastructure this pass cannot stand up; building a fake one
/// here would look done in a grep and not be. <see cref="Create"/> today is
/// invoked manually via <c>RecordRecommendationCommand</c> as a stand-in
/// producer, exactly the same "manual first, event-fed later" restraint
/// Business Intelligence's KpiSnapshot documents for the same reason.
///
/// <see cref="Type"/> is a free-form category (e.g. "ReorderSuggestion",
/// "ProductionScheduleSuggestion") rather than an enum — §2 lists nine open
/// capabilities and hardcoding an enum now would presume which one gets a
/// real producer first. <see cref="ReferenceId"/> is an opaque cross-module
/// reference (e.g. a Product id for a reorder suggestion), never
/// existence-validated here — same convention as Quality's Inspection.ReferenceId.
/// <see cref="ModelVersion"/> exists per §5's model-versioning governance
/// requirement even though no real model produces it yet.
/// </summary>
public sealed class Recommendation : TenantAggregateRoot
{
    public string Type { get; private set; } = default!;
    public Guid ReferenceId { get; private set; }
    public string Summary { get; private set; } = default!;
    public string ModelVersion { get; private set; } = default!;
    public RecommendationStatus Status { get; private set; }
    public DateTimeOffset? DecidedAt { get; private set; }

    private Recommendation() { }

    public static Recommendation Create(Guid companyId, string type, Guid referenceId, string summary, string modelVersion)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new ArgumentException("Recommendation type is required.", nameof(type));
        if (referenceId == Guid.Empty)
            throw new ArgumentException("Reference id is required.", nameof(referenceId));
        if (string.IsNullOrWhiteSpace(summary))
            throw new ArgumentException("A summary explaining the recommendation is required.", nameof(summary));
        if (string.IsNullOrWhiteSpace(modelVersion))
            throw new ArgumentException("Model version is required.", nameof(modelVersion));

        var recommendation = new Recommendation
        {
            CompanyId = companyId,
            Type = type.Trim(),
            ReferenceId = referenceId,
            Summary = summary.Trim(),
            ModelVersion = modelVersion.Trim(),
            Status = RecommendationStatus.Pending,
        };

        recommendation.Raise(new RecommendationCreated(recommendation.Id, companyId, recommendation.Type, referenceId));
        return recommendation;
    }

    /// <summary>Requires the recommendation to still be Pending — same "one clear starting state" discipline as MaintenanceRequest.Start/LeaveRequest.Approve.</summary>
    public void Accept()
    {
        if (Status != RecommendationStatus.Pending)
            throw new InvalidOperationException($"Only a Pending recommendation can be accepted (current status: {Status}).");

        Status = RecommendationStatus.Accepted;
        DecidedAt = DateTimeOffset.UtcNow;
        Raise(new RecommendationAccepted(Id, CompanyId, Type, ReferenceId));
    }

    public void Dismiss()
    {
        if (Status != RecommendationStatus.Pending)
            throw new InvalidOperationException($"Only a Pending recommendation can be dismissed (current status: {Status}).");

        Status = RecommendationStatus.Dismissed;
        DecidedAt = DateTimeOffset.UtcNow;
    }
}
