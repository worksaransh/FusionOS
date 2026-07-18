using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler : IRequestHandler<UpdateSupplierCommand, SupplierDto>
{
    private readonly ISupplierRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateSupplierCommandHandler(ISupplierRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierDto> Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await _repository.GetByIdAsync(request.CompanyId, request.SupplierId, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier '{request.SupplierId}' was not found.");

        supplier.UpdateDetails(request.Name, request.ContactEmail, request.ContactPhone);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SupplierDto(supplier.Id, supplier.Name, supplier.Code, supplier.ContactEmail, supplier.ContactPhone, supplier.IsActive, supplier.CreatedAt);
    }
}
