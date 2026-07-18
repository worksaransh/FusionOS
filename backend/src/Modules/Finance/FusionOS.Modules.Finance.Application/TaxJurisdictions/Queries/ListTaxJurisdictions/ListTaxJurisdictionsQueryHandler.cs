using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Queries.ListTaxJurisdictions;

public sealed class ListTaxJurisdictionsQueryHandler : IRequestHandler<ListTaxJurisdictionsQuery, PagedResult<TaxJurisdictionDto>>
{
    private readonly ITaxJurisdictionRepository _repository;

    public ListTaxJurisdictionsQueryHandler(ITaxJurisdictionRepository repository) => _repository = repository;

    public async Task<PagedResult<TaxJurisdictionDto>> Handle(ListTaxJurisdictionsQuery request, CancellationToken cancellationToken)
    {
        var jurisdictions = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = jurisdictions.Select(CreateTaxJurisdictionCommandHandler.MapToDto).ToList();

        return new PagedResult<TaxJurisdictionDto>(dtos, request.Page, request.PageSize, total);
    }
}
