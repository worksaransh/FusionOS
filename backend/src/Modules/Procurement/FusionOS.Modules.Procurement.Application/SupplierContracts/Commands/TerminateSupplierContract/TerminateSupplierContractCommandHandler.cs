using FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.CreateSupplierContract;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;
using FusionOS.Modules.Procurement.Application.Suppliers.Contracts;
using MediatR;

namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.TerminateSupplierContract;

public sealed class TerminateSupplierContractCommandHandler : IRequestHandler<TerminateSupplierContractCommand, SupplierContractDto>
{
    private readonly ISupplierContractRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public TerminateSupplierContractCommandHandler(ISupplierContractRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SupplierContractDto> Handle(TerminateSupplierContractCommand request, CancellationToken cancellationToken)
    {
        var contract = await _repository.GetByIdAsync(request.CompanyId, request.SupplierContractId, cancellationToken)
            ?? throw new KeyNotFoundException($"Supplier contract '{request.SupplierContractId}' was not found.");

        contract.Terminate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSupplierContractCommandHandler.MapToDto(contract);
    }
}
