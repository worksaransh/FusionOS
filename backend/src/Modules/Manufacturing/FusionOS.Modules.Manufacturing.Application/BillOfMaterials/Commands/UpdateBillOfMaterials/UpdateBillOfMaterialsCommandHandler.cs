using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.UpdateBillOfMaterials;

public sealed class UpdateBillOfMaterialsCommandHandler : IRequestHandler<UpdateBillOfMaterialsCommand, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBillOfMaterialsCommandHandler(IBillOfMaterialsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(UpdateBillOfMaterialsCommand request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        bom.UpdateHeader(request.Name, request.ProductId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
