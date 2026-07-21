using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.ReorderRoutingOperations;

public sealed class ReorderRoutingOperationsCommandHandler : IRequestHandler<ReorderRoutingOperationsCommand, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReorderRoutingOperationsCommandHandler(IBillOfMaterialsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(ReorderRoutingOperationsCommand request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        bom.ReorderOperations(request.OrderedOperationIds);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
