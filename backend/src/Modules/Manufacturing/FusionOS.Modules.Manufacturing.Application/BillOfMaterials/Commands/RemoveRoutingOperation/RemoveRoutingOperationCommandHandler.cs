using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.RemoveRoutingOperation;

public sealed class RemoveRoutingOperationCommandHandler : IRequestHandler<RemoveRoutingOperationCommand, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveRoutingOperationCommandHandler(IBillOfMaterialsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(RemoveRoutingOperationCommand request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        bom.RemoveOperation(request.OperationId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
