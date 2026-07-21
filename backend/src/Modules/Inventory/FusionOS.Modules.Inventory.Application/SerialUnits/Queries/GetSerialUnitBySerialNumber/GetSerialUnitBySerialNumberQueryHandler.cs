using FusionOS.Modules.Inventory.Application.SerialUnits.Contracts;
using MediatR;

namespace FusionOS.Modules.Inventory.Application.SerialUnits.Queries.GetSerialUnitBySerialNumber;

public sealed class GetSerialUnitBySerialNumberQueryHandler : IRequestHandler<GetSerialUnitBySerialNumberQuery, SerialUnitDto?>
{
    private readonly ISerialUnitRepository _repository;

    public GetSerialUnitBySerialNumberQueryHandler(ISerialUnitRepository repository) => _repository = repository;

    public async Task<SerialUnitDto?> Handle(GetSerialUnitBySerialNumberQuery request, CancellationToken cancellationToken)
    {
        var unit = await _repository.GetBySerialNumberAsync(request.CompanyId, request.SerialNumber, cancellationToken);
        return unit is null ? null : SerialUnitMapper.ToDto(unit);
    }
}
