using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Queries.ListBankStatementLines;

/// <summary>IsReconciled is an optional filter — null lists every line for the bank account, true/false narrows to only reconciled or only unreconciled lines.</summary>
public sealed record ListBankStatementLinesQuery(Guid CompanyId, Guid BankAccountId, bool? IsReconciled = null, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<BankStatementLineDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.bank-statement-line.read" };
}
