using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.DeactivateMachine;

public sealed class DeactivateMachineCommandHandler : IRequestHandler<DeactivateMachineCommand, MachineDto>
{
    private readonly IMachineRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateMachineCommandHandler(IMachineRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MachineDto> Handle(DeactivateMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = await _repository.GetByIdAsync(request.CompanyId, request.MachineId, cancellationToken)
            ?? throw new KeyNotFoundException($"Machine '{request.MachineId}' was not found.");

        machine.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MachineMapper.ToDto(machine);
    }
}
