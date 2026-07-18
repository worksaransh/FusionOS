using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Quotations.Queries.ListQuotations;

public sealed class ListQuotationsQueryHandler : IRequestHandler<ListQuotationsQuery, PagedResult<QuotationDto>>
{
    private readonly IQuotationRepository _repository;

    public ListQuotationsQueryHandler(IQuotationRepository repository) => _repository = repository;

    public async Task<PagedResult<QuotationDto>> Handle(ListQuotationsQuery request, CancellationToken cancellationToken)
    {
        var quotations = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = quotations.Select(CreateQuotationCommandHandler.MapToDto).ToList();

        return new PagedResult<QuotationDto>(dtos, request.Page, request.PageSize, total);
    }
}
