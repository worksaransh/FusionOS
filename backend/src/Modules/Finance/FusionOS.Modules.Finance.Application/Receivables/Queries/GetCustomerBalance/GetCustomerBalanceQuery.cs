using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.Receivables.Contracts;

namespace FusionOS.Modules.Finance.Application.Receivables.Queries.GetCustomerBalance;

public sealed record GetCustomerBalanceQuery(Guid CompanyId, Guid CustomerId) : IQuery<CustomerBalanceDto>;
