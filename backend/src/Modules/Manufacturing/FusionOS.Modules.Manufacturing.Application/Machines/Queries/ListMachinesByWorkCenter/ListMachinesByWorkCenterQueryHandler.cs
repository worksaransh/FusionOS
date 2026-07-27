using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Queries.ListMachinesByWorkCenter;

public sealed class ListMachinesByWorkCenterQueryHandler : IRequestHandler<ListMachinesByWorkCenterQuery, IReadOnlyList<MachineDto>>
{
    private readonly IMachineRepository _repository;

    public ListMachinesByWorkCenterQueryHandler(IMachineRepository repository) => _repository = repository;

    public async Task<IReadOnlyList<MachineDto>> Handle(ListMachinesByWorkCenterQuery request, CancellationToken cancellationToken)
    {
        var machines = await _repository.ListByWorkCenterIdAsync(request.CompanyId, request.WorkCenterId, cancellationToken);
        return machines.Select(MachineMapper.ToDto).ToList();
    }
}
