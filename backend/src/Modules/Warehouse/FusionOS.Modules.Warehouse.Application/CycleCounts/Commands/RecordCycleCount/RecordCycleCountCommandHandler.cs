using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.RecordCycleCount;

public sealed class RecordCycleCountCommandHandler : IRequestHandler<RecordCycleCountCommand, CycleCountDto>
{
    private readonly ICycleCountRepository _repository;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public RecordCycleCountCommandHandler(ICycleCountRepository repository, FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CycleCountDto> Handle(RecordCycleCountCommand request, CancellationToken cancellationToken)
    {
        var cycleCount = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (cycleCount is null || cycleCount.CompanyId != request.CompanyId)
        {
            throw new KeyNotFoundException("Cycle count not found.");
        }

        cycleCount.RecordCount(request.CountedQuantity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return StartCycleCountCommandHandler.Map(cycleCount);
    }
}
