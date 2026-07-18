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

    private Opportunity() { }

    public static Opportunity Create(Guid companyId, Guid leadId, string name, string customerName, string? contactEmail, decimal estimatedValue)
    {
        if (leadId == Guid.Empty)
            throw new ArgumentException("Lead id is required.", nameof(leadId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Opportunity name is required.", nameof(name));
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException("Customer name is required.", nameof(customerName));
        if (estimatedValue < 0)
            throw new ArgumentException("Estimated value cannot be negative.", nameof(estimatedValue));

        var opportunity = new Opportunity
        {
            CompanyId = companyId,
            LeadId = leadId,
            Name = name.Trim(),
            CustomerName = customerName.Trim(),
            ContactEmail = string.IsNullOrWhiteSpace(contactEmail) ? null : contactEmail.Trim().ToLowerInvariant(),
            EstimatedValue = estimatedValue,
            Stage = OpportunityStage.Open,
        };

        opportunity.Raise(new OpportunityCreated(opportunity.Id, companyId, leadId, estimatedValue));
        return opportunity;
    }

    /// <summary>
    /// Open → Won: records the customer code the new Sales Customer will take and raises
    /// <see cref="OpportunityWon"/> for Sales to create it.
    /// </summary>
    public void Win(string customerCode)
    {
        if (Stage != OpportunityStage.Open)
            throw new InvalidOperationException($"Only an Open opportunity can be won (current stage: {Stage}).");
        if (string.IsNullOrWhiteSpace(customerCode))
            throw new ArgumentException("A customer code is required to win an opportunity (it becomes the new customer's code).", nameof(customerCode));

        Stage = OpportunityStage.Won;
        CustomerCode = customerCode.Trim().ToUpperInvariant();
        Raise(new OpportunityWon(Id, CompanyId, CustomerName, CustomerCode, ContactEmail));
    }

    /// <summary>Open → Lost: a real, recorded outcome (a sales team wants to know its loss rate), not a delete.</summary>
    public void Lose()
    {
        if (Stage != OpportunityStage.Open)
            throw new InvalidOperationException($"Only an Open opportunity can be marked lost (current stage: {Stage}).");

        Stage = OpportunityStage.Lost;
    }
}
