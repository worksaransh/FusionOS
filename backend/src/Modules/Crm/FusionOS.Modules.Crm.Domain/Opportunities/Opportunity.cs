using FusionOS.SharedKernel;
using FusionOS.Modules.Crm.Domain.Opportunities.Events;

namespace FusionOS.Modules.Crm.Domain.Opportunities;

/// <summary>
/// Phase 4 — CRM. A deal in the pipeline, opened from a qualified <c>Lead</c>. Carries a
/// snapshot of the prospect's name/email (so winning can create a Sales Customer without a
/// cross-module read) plus an estimated value and an Open → Won / Lost stage. Winning
/// raises <see cref="OpportunityWon"/>, which Sales consumes to create the actual Customer
/// — this aggregate never touches Sales directly.
///
/// <see cref="LeadId"/> is a same-module reference (validated by the command handler that
/// opens the opportunity). The Customer created downstream is keyed by
/// <see cref="CustomerCode"/>, chosen when the deal is won.
/// </summary>
public sealed class Opportunity : TenantAggregateRoot
{
    public Guid LeadId { get; private set; }
    public string Name { get; private set; } = default!;
    public string CustomerName { get; private set; } = default!;
    public string? ContactEmail { get; private set; }
    public decimal EstimatedValue { get; private set; }
    public OpportunityStage Stage { get; private set; }
    public string? CustomerCode { get; private set; }

    /// <summary>Win likelihood 0-100, nullable (forecasting is optional) — set at creation or via <see cref="UpdateForecast"/>.</summary>
    public decimal? Probability { get; private set; }

    /// <summary>Forecast close date, nullable — set at creation or via <see cref="UpdateForecast"/>.</summary>
    public DateTimeOffset? ExpectedCloseDate { get; private set; }

    /// <summary>
    /// EstimatedValue weighted by Probability (defaulting to 100, i.e. unweighted, when no
    /// probability has been set) — not persisted, computed on read like other derived values
    /// in this codebase.
    /// </summary>
    public decimal WeightedValue => EstimatedValue * (Probability ?? 100m) / 100m;

    /// <summary>
    /// Optional same-module reference to an Account (Phase 4 CRM depth pass) — same
    /// nullable, handler-validated reference convention as Contact.AccountId
    /// and Lead.AccountId, set via <see cref="AssignAccount"/>.
    /// </summary>
    public Guid? AccountId { get; private set; }

    /// <summary>
    /// Optional opaque reference into Core's own User — the sales rep who owns this
    /// opportunity ("territory via ownership"). Never existence-validated here, same
    /// "opaque cross-module reference" convention as MaintenanceRequest.
    /// AssignedTechnicianUserId — set via <see cref="AssignOwner"/>.
    /// </summary>
    public Guid? OwnerId { get; private set; }

    private Opportunity() { }

    public static Opportunity Create(
        Guid companyId,
        Guid leadId,
        string name,
        string customerName,
        string? contactEmail,
        decimal estimatedValue,
        decimal? probability = null,
        DateTimeOffset? expectedCloseDate = null)
    {
        if (leadId == Guid.Empty)
            throw new ArgumentException("Lead id is required.", nameof(leadId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Opportunity name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));
        if (estimatedValue < 0)
            throw new ArgumentException("Estimated value cannot be negative.", nameof(estimatedValue));
        if (probability is < 0 or > 100)
            throw new ArgumentException("Probability must be between 0 and 100.", nameof(probability));

        var opportunity = new Opportunity
        {
            CompanyId = companyId,
            LeadId = leadId,
            Name = name.Trim(),
            CustomerName = customerName.Trim(),
            ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim().ToLowerInvariant(),
            EstimatedValue = estimatedValue,
            Stage = OpportunityStage.Open,
            Probability = probability,
            ExpectedCloseDate = expectedCloseDate,
        };

        opportunity.Raise(new OpportunityCreated(opportunity.Id, companyId, leadId, estimatedValue));
        return opportunity;
    }

    /// <summary>
    /// Moves the deal between the non-terminal pipeline stages (Open/Prospecting/
    /// Qualification/Proposal/Negotiation). Deliberately does NOT accept Won/Lost — those
    /// remain exclusive to <see cref="Win"/>/<see cref="Lose"/>, which additionally carry
    /// their own side effects (customer code, domain event). A closed opportunity's stage is
    /// immutable: once Won/Lost, it cannot be moved anywhere, including back to a pipeline stage.
    /// </summary>
    public void ChangeStage(OpportunityStage newStage)
    {
        if (Stage is OpportunityStage.Won or OpportunityStage.Lost)
            throw new InvalidOperationException($"A closed opportunity's stage is immutable (current stage: {Stage}).");
        if (newStage is OpportunityStage.Won or OpportunityStage.Lost)
            throw new InvalidOperationException("Use Win/Lose to close an opportunity, not ChangeStage.");

        Stage = newStage;
    }

    /// <summary>Sets the forecast fields — usable at any point before the deal closes; a closed opportunity's forecast is left as-is (historical).</summary>
    public void UpdateForecast(decimal? probability, DateTimeOffset? expectedCloseDate)
    {
        if (Stage is OpportunityStage.Won or OpportunityStage.Lost)
            throw new InvalidOperationException($"A closed opportunity's forecast can no longer be edited (current stage: {Stage}).");
        if (probability is < 0 or > 100)
            throw new ArgumentException("Probability must be between 0 and 100.", nameof(probability));

        Probability = probability;
        ExpectedCloseDate = expectedCloseDate;
    }

    /// <summary>
    /// Won: records the customer code the new Sales Customer will take and raises
    /// <see cref="OpportunityWon"/> for Sales to create it. Allowed from any non-terminal
    /// stage (Open or any pipeline stage reached via <see cref="ChangeStage"/>), not just Open.
    /// </summary>
    public void Win(string customerCode)
    {
        if (Stage is OpportunityStage.Won or OpportunityStage.Lost)
            throw new InvalidOperationException($"Only an open (non-terminal) opportunity can be won (current stage: {Stage}).");
        if (string.IsNullOrWhiteSpace(customerCode))
            throw new ArgumentException("A customer code is required to win an opportunity (it becomes the new customer's code).", nameof(customerCode));

        Stage = OpportunityStage.Won;
        CustomerCode = customerCode.Trim().ToUpperInvariant();
        Raise(new OpportunityWon(Id, CompanyId, CustomerName, CustomerCode, ContactEmail));
    }

    /// <summary>Lost: a real, recorded outcome (a sales team wants to know its loss rate), not a delete. Allowed from any non-terminal stage.</summary>
    public void Lose()
    {
        if (Stage is OpportunityStage.Won or OpportunityStage.Lost)
            throw new InvalidOperationException($"Only an open (non-terminal) opportunity can be marked lost (current stage: {Stage}).");

        Stage = OpportunityStage.Lost;
    }

    /// <summary>Sets or clears (pass null) which Account this opportunity's organization corresponds to.</summary>
    public void AssignAccount(Guid? accountId) => AccountId = accountId;

    /// <summary>Sets or clears (pass null) which Core user owns/works this opportunity.</summary>
    public void AssignOwner(Guid? ownerId) => OwnerId = ownerId;
}
