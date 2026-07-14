using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler : IRequestHandler<CreateSupplierCommand, SupplierDto>
{
    private readonly ISupplierRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierCommandHandler(ISupplierRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierDto> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.CodeExistsAsync(request.CompanyId, request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Supplier code '{request.Code}' already exists for this company."),
            });
        }

        var supplier = Domain.Suppliers.Supplier.Create(request.CompanyId, request.Name, request.Code, request.ContactEmail, request.ContactPhone);

        await _repository.AddAsync(supplier, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new SupplierDto(supplier.Id, supplier.Name, supplier.Code, supplier.ContactEmail, supplier.ContactPhone, supplier.IsActive, supplier.CreatedAt);
    }
}
