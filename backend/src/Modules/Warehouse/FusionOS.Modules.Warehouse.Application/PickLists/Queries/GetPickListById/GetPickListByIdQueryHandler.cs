using FusionOS.Modules.Warehouse.Application.PickLists.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.PickLists.Queries.GetPickListById;

public sealed class GetPickListByIdQueryHandler : IRequestHandler<GetPickListByIdQuery, PickListDto?>
{
    private readonly IPickListRepository _repository;

    public GetPickListByIdQueryHandler(IPickListRepository repository) => _repository = repository;

    public async Task<PickListDto?> Handle(GetPickListByIdQuery request, CancellationToken cancellationToken)
    {
        var pickList = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (pickList is null || pickList.CompanyId != request.CompanyId)
            return null;

        return PickListMapper.MapToDto(pickList);
    }
}
