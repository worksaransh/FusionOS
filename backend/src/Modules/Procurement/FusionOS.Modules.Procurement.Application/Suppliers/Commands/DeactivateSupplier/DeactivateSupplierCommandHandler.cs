using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.DeactivateSupplier;

public sealed class DeactivateSupplierCommandHandler : IRequestHandler<DeactivateSupplierCommand, SupplierDto>
{
    private readonly ISupplierRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSupplierCommandHandler(ISupplierRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierDto> Handle(DeactivateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.CompanyId, request.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier '{request.SupplierId}' was not found.");

        supplier.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SupplierDto(supplier.Id, supplier.Name, supplier.Code, supplier.ContactEmail, supplier.ContactPhone, supplier.IsActive, supplier.CreatedAt);
    }
}
