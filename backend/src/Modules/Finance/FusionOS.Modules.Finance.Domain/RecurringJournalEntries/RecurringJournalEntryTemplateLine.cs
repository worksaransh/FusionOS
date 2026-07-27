namespace FusionOS.Modules.Finance.Domain.RecurringJournalEntries;

/// <summary>
/// A line item within a RecurringJournalEntryTemplate aggregate — same "line's lifecycle is
/// owned entirely by its parent" exception to the audit/tenant-columns rule that
/// JournalEntryLine documents (04_DATABASE_GUIDELINES.md §3). AccountId/CostCenterId are the
/// same same-module references JournalEntryLine carries, validated by the command handler
/// before the aggregate is created — not by an FK constraint.
/// </summary>
public sealed class RecurringJournalEntryTemplateLine
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public Guid? CostCenterId { get; private set; }

    private RecurringJournalEntryTemplateLine() { }

    /// <summary>Same validation as JournalEntryLine.Create (minus Description, which this line does not carry).</summary>
    internal static RecurringJournalEntryTemplateLine Create(Guid accountId, decimal debit, decimal credit, Guid? costCenterId)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account id is required.", nameof(accountId));
        if (debit < 0 || credit < 0)
            throw new ArgumentException("Debit and credit cannot be negative.");
        if (debit > 0 && credit > 0)
            throw new ArgumentException("A line cannot have both a debit and a credit amount.");
        if (debit == 0 && credit == 0)
            throw new ArgumentException("A line must have either a debit or a credit amount.");
        if (costCenterId == Guid.Empty)
            throw new ArgumentException("Cost center id, when supplied, cannot be empty.", nameof(costCenterId));

        return new RecurringJournalEntryTemplateLine
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Debit = debit,
            Credit = credit,
            CostCenterId = costCenterId,
        };
    }
}
