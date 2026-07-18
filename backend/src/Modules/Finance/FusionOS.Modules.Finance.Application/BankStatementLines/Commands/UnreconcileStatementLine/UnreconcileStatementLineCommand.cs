using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.UnreconcileStatementLine;

/// <summary>The inverse of ReconcileStatementLineCommand — see BankStatementLine.Unreconcile's own doc comment for why reconciliation is a toggle rather than a one-way state.</summary>
public sealed record UnreconcileStatementLineCommand(Guid CompanyId, Guid StatementLineId)
    : ICommand<BankStatementLineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.bank-statement-line.reconcile" };
    public string EntityType => nameof(Domain.BankStatementLines.BankStatementLine);
    public Guid EntityId => StatementLineId;
    public string Action => "Unreconciled";
}
