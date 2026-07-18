using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.BankStatementLines.Contracts;

namespace FusionOS.Modules.Finance.Application.BankStatementLines.Queries.GetReconciliationSummary;

public sealed record GetReconciliationSummaryQuery(Guid CompanyId, Guid BankAccountId)
    : IQuery<ReconciliationSummaryDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.bank-statement-line.read" };
}
