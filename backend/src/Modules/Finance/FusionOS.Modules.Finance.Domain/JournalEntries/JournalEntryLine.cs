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
    /// Optional dimension references for multidimensional accounting (CostCenter, Department, Project, Location).
    /// </summary>
    public Guid? CostCenterId { get; private set; }
    public Guid? DepartmentId { get; private set; }
    public Guid? ProjectId { get; private set; }
    public Guid? LocationId { get; private set; }

    /// <summary>
    /// Optional foreign-currency tag for a line whose Debit/Credit (always
    /// recorded in the company's base currency, per the rest of this ledger)
    /// represents a converted foreign-currency transaction — e.g. a supplier
    /// bill or customer invoice denominated in USD/EUR. ForeignAmount is the
    /// original amount in ForeignCurrencyCode at the rate in effect when this
    /// line was created; both are null for ordinary base-currency lines. This
    /// is what RevalueForeignCurrencyBalancesCommandHandler needs to exist
    /// before it can compute a real gain/loss — only lines carrying both
    /// fields are eligible for revaluation, so historical entries created
    /// before this field existed are correctly excluded rather than silently
    /// mis-revalued from an assumed rate.
    /// </summary>
    public string? ForeignCurrencyCode { get; private set; }
    public decimal? ForeignAmount { get; private set; }

    private JournalEntryLine() { }

    internal static JournalEntryLine Create(
        Guid accountId,
        decimal debit,
        decimal credit,
        string? description,
        Guid? costCenterId = null,
        Guid? departmentId = null,
        Guid? projectId = null,
        Guid? locationId = null,
        string? foreignCurrencyCode = null,
        decimal? foreignAmount = null)
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
        if (departmentId == Guid.Empty)
            throw new ArgumentException("Department id, when supplied, cannot be empty.", nameof(departmentId));
        if (projectId == Guid.Empty)
            throw new ArgumentException("Project id, when supplied, cannot be empty.", nameof(projectId));
        if (locationId == Guid.Empty)
            throw new ArgumentException("Location id, when supplied, cannot be empty.", nameof(locationId));
        if (string.IsNullOrWhiteSpace(foreignCurrencyCode) != !foreignAmount.HasValue)
            throw new ArgumentException("ForeignCurrencyCode and ForeignAmount must be supplied together, or not at all.");
        if (foreignAmount.HasValue && foreignAmount.Value <= 0)
            throw new ArgumentException("Foreign amount, when supplied, must be positive.", nameof(foreignAmount));

        return new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Debit = debit,
            Credit = credit,
            Description = description?.Trim(),
            CostCenterId = costCenterId,
            DepartmentId = departmentId,
            ProjectId = projectId,
            LocationId = locationId,
            ForeignCurrencyCode = string.IsNullOrWhiteSpace(foreignCurrencyCode) ? null : foreignCurrencyCode.Trim().ToUpperInvariant(),
            ForeignAmount = foreignAmount,
        };
    }
}
