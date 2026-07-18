namespace FusionOS.Modules.Finance.Domain.JournalEntries;

/// <summary>
/// A line item within a JournalEntry aggregate. Documented, reviewed exception to
/// the "every table has audit/tenant columns" rule (04_DATABASE_GUIDELINES.md §3),
/// same reasoning as PurchaseOrderLine/SalesOrderLine/GoodsReceiptLine: a line's
/// lifecycle is owned entirely by its parent JournalEntry. AccountId IS a real,
/// same-module foreign key into Account (unlike ProductId in Procurement/Sales
/// lines), validated by the command handler before the aggregate is created.
/// </summary>
public sealed class JournalEntryLine
{
    public Guid Id { get; private set; }
    public Guid AccountId { get; private set; }
    public decimal Debit { get; private set; }
    public decimal Credit { get; private set; }
    public string? Description { get; private set; }

    /// <summary>
    /// Optional same-module reference to a CostCenter, enabling cost-center-aware
    /// reporting (Budget vs-actual). Nullable because a line need not carry a cost
    /// center; existence, when supplied, is validated by the command handler before
    /// the aggregate is created — the same discipline as AccountId above. No FK join
    /// is declared across the aggregate boundary; this is a plain reference column.
    /// </summary>
    public Guid? CostCenterId { get; private set; }

    private JournalEntryLine() { }

    internal static JournalEntryLine Create(Guid accountId, decimal debit, decimal credit, string? description, Guid? costCenterId = null)
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

        return new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Debit = debit,
            Credit = credit,
            Description = description?.Trim(),
            CostCenterId = costCenterId,
        };
    }
}
