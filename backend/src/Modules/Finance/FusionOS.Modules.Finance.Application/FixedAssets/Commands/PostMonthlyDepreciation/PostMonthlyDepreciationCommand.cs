using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.JournalEntries.Contracts;

namespace FusionOS.Modules.Finance.Application.FixedAssets.Commands.PostMonthlyDepreciation;

/// <summary>
/// Posts one month of straight-line depreciation for a FixedAsset to the General
/// Ledger as a single, real (immediately Posted) JournalEntry: Debit the caller-
/// supplied depreciation-expense account, Credit the asset's
/// AccumulatedDepreciationAccountId. This composes two things that already exist —
/// the straight-line monthly figure GetDepreciationScheduleQueryHandler computes
/// ((AcquisitionCost - SalvageValue) / UsefulLifeMonths) and JournalEntry's own
/// Create/Post workflow — rather than introducing any new aggregate or persisted
/// depreciation-schedule table. It is the "depreciation-run process that posts real
/// JournalEntries" both FixedAsset.cs and GetDepreciationScheduleQueryHandler.cs
/// flagged as the explicit future follow-up to their read-only calculation.
///
/// The depreciation-expense account is a command parameter, not a FixedAsset field:
/// the aggregate only ever stored AccumulatedDepreciationAccountId (the credit side),
/// so the expense (debit) account is chosen at posting time.
///
/// <b>Deliberately scoped out (one-slice discipline):</b> this posts exactly one
/// month's amount per invocation and does NOT track how many months have already
/// been posted — it will not, by itself, stop a caller posting more than
/// UsefulLifeMonths times or twice for the same period. A recurring scheduler and a
/// posted-to-date guard are a distinct follow-up. Also, the JournalEntry's EntryDate
/// follows JournalEntry's existing UtcNow convention (its Create does not accept a
/// date); PeriodEnd is recorded in the entry Reference so the period the posting
/// represents is still captured.
/// </summary>
public sealed record PostMonthlyDepreciationCommand(
    Guid CompanyId,
    Guid FixedAssetId,
    Guid DepreciationExpenseAccountId,
    DateTimeOffset PeriodEnd)
    : ICommand<JournalEntryDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.fixed-asset.depreciate" };
    public string EntityType => nameof(Domain.FixedAssets.FixedAsset);
    public Guid EntityId => FixedAssetId;
    public string Action => "DepreciationPosted";
}
