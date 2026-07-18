using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.ReconcileStatementLine;

/// <summary>MatchedJournalEntryId is an optional, user-picked link — no auto-matching algorithm (see BankStatementLine's class doc comment). Not validated for existence here (same "opaque reference" precedent as ApLedgerEntry's SupplierId/PurchaseOrderId).</summary>
public sealed record ReconcileStatementLineCommand(Guid CompanyId, Guid StatementLineId, Guid? MatchedJournalEntryId)
    : ICommand<BankStatementLineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.bank-statement-line.reconcile" };
    public string EntityType => nameof(Domain.BankStatementLines.BankStatementLine);
    public Guid EntityId => StatementLineId;
    public string Action => "Reconciled";
}
