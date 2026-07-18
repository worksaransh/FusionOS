using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Commands.PackPickList;

public sealed class PackPickListCommandHandler : IRequestHandler<PackPickListCommand, PickListDto>
{
    private readonly IPickListRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public PackPickListCommandHandler(IPickListRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PickListDto> Handle(PackPickListCommand request, CancellationToken cancellationToken)
    {
        var pickList = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (pickList is null || pickList.CompanyId != request.CompanyId)
        {
            throw new KeyNotFoundException($"Pick list '{request.Id}' was not found.");
        }

        pickList.Pack();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return PickListMapper.MapToDto(pickList);
    }
}
