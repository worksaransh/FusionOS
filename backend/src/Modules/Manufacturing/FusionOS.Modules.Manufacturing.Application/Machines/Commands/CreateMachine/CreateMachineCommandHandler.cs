using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Manufacturing.Application.BillOfMaterials.Contracts;
using FusionOS.Modules.Manufacturing.Application.Machines.Contracts;
using FusionOS.Modules.Manufacturing.Application.WorkCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Manufacturing.Application.Machines.Commands.CreateMachine;

public sealed class CreateMachineCommandHandler : IRequestHandler<CreateMachineCommand, MachineDto>
{
    private readonly IMachineRepository _machineRepository;
    private readonly IWorkCenterRepository _workCenterRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateMachineCommandHandler(
        IMachineRepository machineRepository,
        IWorkCenterRepository workCenterRepository,
        IUnitOfWork unitOfWork)
    {
        _machineRepository = machineRepository;
        _workCenterRepository = workCenterRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<MachineDto> Handle(CreateMachineCommand request, CancellationToken cancellationToken)
    {
        var workCenter = await _workCenterRepository.GetByIdAsync(request.CompanyId, request.WorkCenterId, cancellationToken)
            ?? throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.WorkCenterId), $"Work center '{request.WorkCenterId}' does not exist for this company."),
            });

        var code = request.Code?.Trim().ToUpperInvariant() ?? string.Empty;
        if (await _machineRepository.CodeExistsAsync(request.CompanyId, code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"A machine with code '{code}' already exists for this company."),
            });
        }

        var machine = Domain.Machines.Machine.Create(request.CompanyId, request.Code, request.Name, workCenter.Id);

        await _machineRepository.AddAsync(machine, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MachineMapper.ToDto(machine);
    }
}
