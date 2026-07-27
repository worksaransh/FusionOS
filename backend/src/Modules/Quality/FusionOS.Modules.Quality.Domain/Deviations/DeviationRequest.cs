using FusionOS.SharedKernel;

namespace FusionOS.Modules.Quality.Domain.Deviations;

/// <summary>
/// A request to accept an out-of-spec result anyway (a "waiver") — typically raised against a
/// failed/out-of-spec <see cref="Inspections.Inspection"/>, though InspectionId is optional
/// since a deviation can also be requested for a known-in-advance planned deviation with no
/// failed inspection behind it yet (e.g. "we know this batch will run outside spec, requesting
/// pre-approval"). InspectionId is a same-module reference (Inspection lives in this same
/// Quality module) but deliberately NOT existence-validated here (unlike CorrectiveAction's
/// NonConformanceReportId) — it is optional precisely to support the no-inspection-yet case
/// above, so a handler-level existence check would have to be conditional on it being supplied;
/// left as an opaque reference like Inspection.ReferenceId is for its own cross-module ids,
/// simplest given the field is optional either way.
///
/// RequestedByUserId / ApprovedOrRejectedByUserId are opaque cross-module references into
/// Core's User — never existence-validated, same convention as CorrectiveAction.AssignedToUserId.
///
/// Lifecycle: Pending -> Approved or Pending -> Rejected, both terminal — a decision cannot be
/// revisited once made, same one-shot decision discipline as NonConformanceReport.UpdateStatus
/// disallowing any change once Closed.
/// </summary>
public sealed class DeviationRequest : TenantAggregateRoot
{
    public Guid? InspectionId { get; private set; }
    public string Reason { get; private set; } = default!;
    public DeviationRequestStatus Status { get; private set; }
    public Guid RequestedByUserId { get; private set; }
    public Guid? ApprovedOrRejectedByUserId { get; private set; }
    public string? DecisionNote { get; private set; }

    private DeviationRequest() { }

    public static DeviationRequest Create(Guid companyId, Guid? inspectionId, string reason, Guid requestedByUserId)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A reason for the deviation/waiver request is required.", nameof(reason));
        if (requestedByUserId == Guid.Empty)
            throw new ArgumentException("Requested-by user id is required.", nameof(requestedByUserId));
        if (inspectionId == Guid.Empty)
            throw new ArgumentException("Inspection id, when supplied, cannot be empty — omit it entirely when there is no inspection to reference yet.", nameof(inspectionId));

        return new DeviationRequest
        {
            CompanyId = companyId,
            InspectionId = inspectionId,
            Reason = reason.Trim(),
            Status = DeviationRequestStatus.Pending,
            RequestedByUserId = requestedByUserId,
        };
    }

    /// <summary>Approves the request. Requires it to currently be Pending.</summary>
    public void Approve(Guid decidedByUserId, string? note)
    {
        if (Status != DeviationRequestStatus.Pending)
            throw new InvalidOperationException($"Only a Pending deviation request can be approved (current status: {Status}).");
        if (decidedByUserId == Guid.Empty)
            throw new ArgumentException("Decided-by user id is required.", nameof(decidedByUserId));

        Status = DeviationRequestStatus.Approved;
        ApprovedOrRejectedByUserId = decidedByUserId;
        DecisionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }

    /// <summary>Rejects the request. Requires it to currently be Pending.</summary>
    public void Reject(Guid decidedByUserId, string? note)
    {
        if (Status != DeviationRequestStatus.Pending)
            throw new InvalidOperationException($"Only a Pending deviation request can be rejected (current status: {Status}).");
        if (decidedByUserId == Guid.Empty)
            throw new ArgumentException("Decided-by user id is required.", nameof(decidedByUserId));

        Status = DeviationRequestStatus.Rejected;
        ApprovedOrRejectedByUserId = decidedByUserId;
        DecisionNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
    }
}
