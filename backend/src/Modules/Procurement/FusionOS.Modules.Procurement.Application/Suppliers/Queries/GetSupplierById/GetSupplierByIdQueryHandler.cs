using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Queries.GetSupplierById;

public sealed class GetSupplierByIdQueryHandler : IRequestHandler<GetSupplierByIdQuery, SupplierDto>
{
    private readonly ISupplierRepository _repository;

    public GetSupplierByIdQueryHandler(ISupplierRepository repository) => _repository = repository;

    public async Task<SupplierDto> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.CompanyId, request.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier '{request.SupplierId}' was not found.");

        return new SupplierDto(supplier.Id, supplier.Name, supplier.Code, supplier.ContactEmail, supplier.ContactPhone, supplier.IsActive, supplier.CreatedAt);
    }
}
