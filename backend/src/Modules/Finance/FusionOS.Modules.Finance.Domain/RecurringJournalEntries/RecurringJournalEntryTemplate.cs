using FusionOS.SharedKernel;
using FusionOS.Modules.Finance.Domain.RecurringJournalEntries.Events;

namespace FusionOS.Modules.Finance.Domain.RecurringJournalEntries;

/// <summary>
/// New — recurring journal entries (a monthly rent/depreciation-style entry that should
/// repeat), confirmed absent before this feature. A template holds the reusable balanced set
/// of lines (same debit == credit invariant <see cref="JournalEntries.JournalEntry.Create"/>
/// enforces — mirrored here, not shared code, since RecurringJournalEntryTemplateLine and
/// JournalEntryLine are separate value shapes with separate lifecycles) plus a Frequency and a
/// NextDueDate. GenerateJournalEntryFromTemplateCommandHandler is the only thing that turns a
/// template into a real, persisted JournalEntry — creating a template here does not itself post
/// anything to the ledger.
/// </summary>
public sealed class RecurringJournalEntryTemplate : TenantAggregateRoot
{
    private readonly List<RecurringJournalEntryTemplateLine> _lines = new();

    public string Reference { get; private set; } = default!;
    public RecurringJournalEntryFrequency Frequency { get; private set; }
    public DateTimeOffset NextDueDate { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<RecurringJournalEntryTemplateLine> Lines => _lines.AsReadOnly();
    public decimal TotalDebit => _lines.Sum(l => l.Debit);
    public decimal TotalCredit => _lines.Sum(l => l.Credit);

    private RecurringJournalEntryTemplate() { }

    public static RecurringJournalEntryTemplate Create(Guid companyId, string reference, RecurringJournalEntryFrequency frequency, DateTimeOffset nextDueDate, IReadOnlyCollection<RecurringJournalEntryTemplateLineInput> lines)
    {
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("A reference/description is required.", nameof(reference));
        if (lines is null || lines.Count < 2)
            throw new ArgumentException("A recurring journal entry template must have at least two lines.", nameof(lines));

        var template = new RecurringJournalEntryTemplate
        {
            CompanyId = companyId,
            Reference = reference.Trim(),
            Frequency = frequency,
            NextDueDate = nextDueDate,
            IsActive = true,
        };

        foreach (var line in lines)
            template._lines.Add(RecurringJournalEntryTemplateLine.Create(line.AccountId, line.Debit, line.Credit, line.CostCenterId));

        if (template.TotalDebit != template.TotalCredit)
            throw new InvalidOperationException($"Recurring journal entry template is not balanced: total debit {template.TotalDebit} != total credit {template.TotalCredit}.");

        template.Raise(new RecurringJournalEntryTemplateCreated(template.Id, companyId, template.Reference, frequency.ToString()));
        return template;
    }

    /// <summary>Replaces the mutable master-data fields and the entire line set (same "replace, don't patch" shape CreateJournalEntry uses — there is no per-line edit, only a whole-set swap), re-validating balance exactly like Create. Not allowed once deactivated.</summary>
    public void UpdateDetails(string reference, RecurringJournalEntryFrequency frequency, DateTimeOffset nextDueDate, IReadOnlyCollection<RecurringJournalEntryTemplateLineInput> lines)
    {
        if (!IsActive)
            throw new InvalidOperationException("A deactivated recurring journal entry template cannot be updated.");
        if (string.IsNullOrWhiteSpace(reference))
            throw new ArgumentException("A reference/description is required.", nameof(reference));
        if (lines is null || lines.Count < 2)
            throw new ArgumentException("A recurring journal entry template must have at least two lines.", nameof(lines));

        var newLines = lines.Select(l => RecurringJournalEntryTemplateLine.Create(l.AccountId, l.Debit, l.Credit, l.CostCenterId)).ToList();
        var totalDebit = newLines.Sum(l => l.Debit);
        var totalCredit = newLines.Sum(l => l.Credit);
        if (totalDebit != totalCredit)
            throw new InvalidOperationException($"Recurring journal entry template is not balanced: total debit {totalDebit} != total credit {totalCredit}.");

        Reference = reference.Trim();
        Frequency = frequency;
        NextDueDate = nextDueDate;
        _lines.Clear();
        _lines.AddRange(newLines);
    }

    public void Deactivate()
    {
        if (!IsActive)
            throw new InvalidOperationException("The recurring journal entry template is already inactive.");

        IsActive = false;
    }

    /// <summary>
    /// Advances NextDueDate by one Frequency unit — Monthly +1 month, Quarterly +3 months,
    /// Annual +1 year. Called by GenerateJournalEntryFromTemplateCommandHandler immediately
    /// after a real JournalEntry has been generated from this template's current lines, so the
    /// template is positioned for its next occurrence. Does not itself touch IsActive or Lines.
    /// </summary>
    public void AdvanceNextDueDate()
    {
        NextDueDate = Frequency switch
        {
            RecurringJournalEntryFrequency.Monthly => NextDueDate.AddMonths(1),
            RecurringJournalEntryFrequency.Quarterly => NextDueDate.AddMonths(3),
            RecurringJournalEntryFrequency.Annual => NextDueDate.AddYears(1),
            _ => throw new InvalidOperationException($"Unknown recurring journal entry frequency '{Frequency}'."),
        };
    }
}
