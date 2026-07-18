using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.RecordPick;

public sealed class RecordPickCommandHandler : IRequestHandler<RecordPickCommand, PickListDto>
{
    private readonly IPickListRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public RecordPickCommandHandler(IPickListRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PickListDto> Handle(RecordPickCommand request, CancellationToken cancellationToken)
    {
        var pickList = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (pickList is null || pickList.CompanyId != request.CompanyId)
        {
            throw new KeyNotFoundException($"Pick list '{request.Id}' was not found.");
        }

        pickList.RecordPick(request.LineId, request.QuantityPicked);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PickListMapper.MapToDto(pickList);
    }
}
