using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Commands.RecordStatementLine;

/// <summary>
/// Records one manually-entered bank statement line (see
/// BankStatementLine's class doc comment for why there's no bank-feed/
/// file-import path). BankAccountId is checked for existence by
/// RecordStatementLineCommandHandler via IBankAccountRepository.ExistsAsync
/// before the line is created.
/// </summary>
public sealed record RecordStatementLineCommand(Guid CompanyId, Guid BankAccountId, DateTimeOffset TransactionDate, decimal Amount, string Description)
    : ICommand<BankStatementLineDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.bank-statement-line.create" };
    public string EntityType => nameof(Domain.BankStatementLines.BankStatementLine);
    public Guid EntityId { get; init; }
    public string Action => "Recorded";
}
