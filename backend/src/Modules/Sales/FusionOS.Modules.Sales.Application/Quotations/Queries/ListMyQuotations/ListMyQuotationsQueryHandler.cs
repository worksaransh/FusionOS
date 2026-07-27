using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Quotations.Commands.CreateQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Quotations.Queries.ListMyQuotations;

public sealed class ListMyQuotationsQueryHandler : IRequestHandler<ListMyQuotationsQuery, PagedResult<QuotationDto>>
{
    private readonly IQuotationRepository _repository;

    public ListMyQuotationsQueryHandler(IQuotationRepository repository) => _repository = repository;

    public async Task<PagedResult<QuotationDto>> Handle(ListMyQuotationsQuery request, CancellationToken cancellationToken)
    {
        var quotations = await _repository.ListByCustomerAsync(request.CustomerId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountByCustomerAsync(request.CustomerId, cancellationToken);

        var dtos = quotations.Select(CreateQuotationCommandHandler.MapToDto).ToList();

        return new PagedResult<QuotationDto>(dtos, request.Page, request.PageSize, total);
    }
}
