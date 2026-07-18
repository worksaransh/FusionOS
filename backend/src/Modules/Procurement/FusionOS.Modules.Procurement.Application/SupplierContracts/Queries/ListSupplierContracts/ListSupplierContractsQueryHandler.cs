using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.CreateSupplierContract;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Queries.ListSupplierContracts;

public sealed class ListSupplierContractsQueryHandler : IRequestHandler<ListSupplierContractsQuery, PagedResult<SupplierContractDto>>
{
    private readonly ISupplierContractRepository _repository;

    public ListSupplierContractsQueryHandler(ISupplierContractRepository repository) => _repository = repository;

    public async Task<PagedResult<SupplierContractDto>> Handle(ListSupplierContractsQuery request, CancellationToken cancellationToken)
    {
        var contracts = await _repository.ListAsync(request.CompanyId, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, cancellationToken);

        var dtos = contracts.Select(CreateSupplierContractCommandHandler.MapToDto).ToList();

        return new PagedResult<SupplierContractDto>(dtos, request.Page, request.PageSize, total);
    }
}
