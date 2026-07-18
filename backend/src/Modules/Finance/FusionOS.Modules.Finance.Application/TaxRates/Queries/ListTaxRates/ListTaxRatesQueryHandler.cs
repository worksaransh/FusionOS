using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxRates.Commands.CreateTaxRate;
using FusionOS.Modules.Finance.Application.TaxRates.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxRates.Queries.ListTaxRates;

public sealed class ListTaxRatesQueryHandler : IRequestHandler<ListTaxRatesQuery, PagedResult<TaxRateDto>>
{
    private readonly ITaxRateRepository _repository;

    public ListTaxRatesQueryHandler(ITaxRateRepository repository) => _repository = repository;

    public async Task<PagedResult<TaxRateDto>> Handle(ListTaxRatesQuery request, CancellationToken cancellationToken)
    {
        var taxRates = await _repository.ListAsync(request.CompanyId, request.TaxJurisdictionId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.TaxJurisdictionId, cancellationToken);

        var dtos = taxRates.Select(CreateTaxRateCommandHandler.MapToDto).ToList();

        return new PagedResult<TaxRateDto>(dtos, request.Page, request.PageSize, total);
    }
}
