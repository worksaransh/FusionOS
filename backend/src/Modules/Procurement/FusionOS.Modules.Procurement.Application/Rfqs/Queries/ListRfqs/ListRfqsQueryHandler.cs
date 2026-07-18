using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Rfqs.Commands.CreateRfq;
using FusionOS.Modules.Procurement.Application.Rfqs.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Rfqs.Queries.ListRfqs;

public sealed class ListRfqsQueryHandler : IRequestHandler<ListRfqsQuery, PagedResult<RfqDto>>
{
    private readonly IRfqRepository _repository;

    public ListRfqsQueryHandler(IRfqRepository repository) => _repository = repository;

    public async Task<PagedResult<RfqDto>> Handle(ListRfqsQuery request, CancellationToken cancellationToken)
    {
        var rfqs = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = rfqs.Select(CreateRfqCommandHandler.MapToDto).ToList();

        return new PagedResult<RfqDto>(dtos, request.Page, request.PageSize, total);
    }
}
