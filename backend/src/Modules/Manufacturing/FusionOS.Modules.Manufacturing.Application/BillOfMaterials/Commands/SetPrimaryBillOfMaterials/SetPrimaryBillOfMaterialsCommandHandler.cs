using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Commands.SetPrimaryBillOfMaterials;

public sealed class SetPrimaryBillOfMaterialsCommandHandler : IRequestHandler<SetPrimaryBillOfMaterialsCommand, BillOfMaterialsDto>
{
    private readonly IBillOfMaterialsRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPrimaryBillOfMaterialsCommandHandler(IBillOfMaterialsRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BillOfMaterialsDto> Handle(SetPrimaryBillOfMaterialsCommand request, CancellationToken cancellationToken)
    {
        var bom = await _repository.GetByIdAsync(request.CompanyId, request.BillOfMaterialsId, cancellationToken)
            ?? throw new KeyNotFoundException($"Bill of materials '{request.BillOfMaterialsId}' was not found.");

        var activeBoms = await _repository.ListActiveByProductIdAsync(request.CompanyId, bom.ProductId, cancellationToken);
        foreach (var other in activeBoms)
        {
            if (other.Id != bom.Id)
                other.ClearPrimary();
        }

        bom.MarkAsPrimary();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return BillOfMaterialsMapper.ToDto(bom);
    }
}
