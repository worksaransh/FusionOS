using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.CreateSupplierContract;

public sealed class CreateSupplierContractCommandHandler : IRequestHandler<CreateSupplierContractCommand, SupplierContractDto>
{
    private readonly ISupplierContractRepository _repository;
    private readonly ISupplierRepository _supplierRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSupplierContractCommandHandler(ISupplierContractRepository repository, ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _supplierRepository = supplierRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierContractDto> Handle(CreateSupplierContractCommand request, CancellationToken cancellationToken)
    {
        if (!await _supplierRepository.ExistsAsync(request.CompanyId, request.SupplierId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.SupplierId), "Supplier does not exist for this company."),
            });
        }

        var contract = Domain.SupplierContracts.SupplierContract.Create(request.CompanyId, request.SupplierId, request.StartDate, request.EndDate, request.Terms);

        await _repository.AddAsync(contract, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(contract);
    }

    internal static SupplierContractDto MapToDto(Domain.SupplierContracts.SupplierContract contract) => new(
        contract.Id,
        contract.SupplierId,
        contract.StartDate,
        contract.EndDate,
        contract.Terms,
        contract.Status.ToString());
}
