using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Commands.CreateWithholdingTaxRate;
using FusionOS.Modules.Finance.Application.WithholdingTaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.WithholdingTaxRates.Queries.ListWithholdingTaxRates;

public sealed class ListWithholdingTaxRatesQueryHandler : IRequestHandler<ListWithholdingTaxRatesQuery, PagedResult<WithholdingTaxRateDto>>
{
    private readonly IWithholdingTaxRateRepository _repository;

    public ListWithholdingTaxRatesQueryHandler(IWithholdingTaxRateRepository repository) => _repository = repository;

    public async Task<PagedResult<WithholdingTaxRateDto>> Handle(ListWithholdingTaxRatesQuery request, CancellationToken cancellationToken)
    {
        var rates = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        return new PagedResult<WithholdingTaxRateDto>(
            rates.Select(CreateWithholdingTaxRateCommandHandler.MapToDto).ToList(),
            request.Page,
            request.PageSize,
            total);
    }
}
