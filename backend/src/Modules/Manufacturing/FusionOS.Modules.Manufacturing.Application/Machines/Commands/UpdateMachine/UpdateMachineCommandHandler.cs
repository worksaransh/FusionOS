using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.UpdateMachine;

public sealed class UpdateMachineCommandHandler : IRequestHandler<UpdateMachineCommand, MachineDto>
{
    private readonly IMachineRepository _machineRepository;
    private readonly IWorkCenterRepository _workCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateMachineCommandHandler(
        IMachineRepository machineRepository,
        IWorkCenterRepository workCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _machineRepository = machineRepository;
        _workCenterRepository = workCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MachineDto> Handle(UpdateMachineCommand request, CancellationToken cancellationToken)
    {
        var machine = await _machineRepository.GetByIdAsync(request.CompanyId, request.MachineId, cancellationToken)
            ?? throw new KeyNotFoundException($"Machine '{request.MachineId}' was not found.");

        var workCenter = await _workCenterRepository.GetByIdAsync(request.CompanyId, request.WorkCenterId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.WorkCenterId), $"Work center '{request.WorkCenterId}' does not exist for this company."),
            });

        machine.UpdateDetails(request.Name, workCenter.Id);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MachineMapper.ToDto(machine);
    }
}
