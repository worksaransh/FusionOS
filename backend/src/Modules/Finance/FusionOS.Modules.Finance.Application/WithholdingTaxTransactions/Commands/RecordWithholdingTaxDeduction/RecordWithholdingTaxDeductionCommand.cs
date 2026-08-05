using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Contracts;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Commands.RecordWithholdingTaxDeduction;

/// <summary>
/// PayableGlAccountId/WithholdingPayableGlAccountId are supplied by the caller rather than
/// looked up from a company-wide default setting (contrast FinanceSettings' Default*AccountId
/// fields) — this slice deliberately keeps TDS/TCS scoped to "compute the deduction and post
/// the GL entry with accounts the caller specifies," matching how JournalEntry creation
/// itself always takes explicit account ids. Wiring a default-account setting for this is a
/// documented follow-up, not a shape limitation of the domain model.
/// </summary>
public sealed record RecordWithholdingTaxDeductionCommand(
    Guid CompanyId,
    Guid WithholdingTaxRateId,
    string ReferenceType,
    Guid ReferenceId,
    decimal GrossAmount,
    Guid PayableGlAccountId,
    Guid WithholdingPayableGlAccountId)
    : ICommand<WithholdingTaxTransactionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.withholding-tax-transaction.record" };
    public string EntityType => nameof(Domain.WithholdingTaxTransactions.WithholdingTaxTransaction);
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Recorded";
}
