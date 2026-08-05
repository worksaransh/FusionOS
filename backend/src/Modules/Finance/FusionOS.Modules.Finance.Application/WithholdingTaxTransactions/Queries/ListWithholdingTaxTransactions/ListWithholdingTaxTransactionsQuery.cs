using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Contracts;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxTransactions.Queries.ListWithholdingTaxTransactions;

public sealed record ListWithholdingTaxTransactionsQuery(Guid CompanyId, int Page = 1, int PageSize = 25)
    : IQuery<PagedResult<WithholdingTaxTransactionDto>>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.withholding-tax-transaction.read" };
}
