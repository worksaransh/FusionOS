using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Queries.GetSerialUnitById;

public sealed class GetSerialUnitByIdQueryHandler : IRequestHandler<GetSerialUnitByIdQuery, SerialUnitDto?>
{
    private readonly ISerialUnitRepository _repository;

    public GetSerialUnitByIdQueryHandler(ISerialUnitRepository repository) => _repository = repository;

    public async Task<SerialUnitDto?> Handle(GetSerialUnitByIdQuery request, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetByIdAsync(request.CompanyId, request.Id, cancellationToken);
        return unit is null ? null : SerialUnitMapper.ToDto(unit);
    }
}
