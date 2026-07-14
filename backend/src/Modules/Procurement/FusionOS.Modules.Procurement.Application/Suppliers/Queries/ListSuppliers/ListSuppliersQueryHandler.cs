using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Queries.ListSuppliers;

public sealed class ListSuppliersQueryHandler : IRequestHandler<ListSuppliersQuery, PagedResult<SupplierDto>>
{
    private readonly ISupplierRepository _repository;

    public ListSuppliersQueryHandler(ISupplierRepository repository) => _repository = repository;

    public async Task<PagedResult<SupplierDto>> Handle(ListSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await _repository.ListAsync(request.CompanyId, request.Search, request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(request.CompanyId, request.Search, cancellationToken);

        var dtos = suppliers
            .Select(s => new SupplierDto(s.Id, s.Name, s.Code, s.ContactEmail, s.ContactPhone, s.IsActive, s.CreatedAt))
            .ToList();

        return new PagedResult<SupplierDto>(dtos, request.Page, request.PageSize, total);
    }
}
