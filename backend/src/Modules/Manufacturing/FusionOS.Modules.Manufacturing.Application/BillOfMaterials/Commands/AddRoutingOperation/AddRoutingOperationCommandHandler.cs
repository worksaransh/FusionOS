using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.AddRoutingOperation;

public sealed class AddRoutingOperationCommandHandler : IRequestHandler<AddRoutingOperationCommand, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AddRoutingOperationCommandHandler(IBillOfMaterialsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(AddRoutingOperationCommand request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        bom.AddOperation(request.OperationName, request.WorkCenter, request.StandardMinutes);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
