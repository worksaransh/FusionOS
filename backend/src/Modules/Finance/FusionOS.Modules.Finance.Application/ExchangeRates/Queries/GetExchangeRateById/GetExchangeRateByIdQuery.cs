using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.ExchangeRates.Contracts;

namespace FusionOS.Modules.Finance.Application.ExchangeRates.Queries.GetExchangeRateById;

public sealed record GetExchangeRateByIdQuery(Guid CompanyId, Guid ExchangeRateId)
    : IQuery<ExchangeRateDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "finance.exchange-rate.read" };
}
