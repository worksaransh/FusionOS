using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;
using FusionOS.SharedKernel.Context;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;

public sealed class StartCycleCountCommandHandler : IRequestHandler<StartCycleCountCommand, CycleCountDto>
{
    private readonly ICycleCountRepository _repository;
    private readonly ICurrentUserContext _currentUser;
    private readonly FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork _unitOfWork;

    public StartCycleCountCommandHandler(
        ICycleCountRepository repository,
        ICurrentUserContext currentUser,
        FusionOS.Modules.Warehouse.Application.Warehouses.Contracts.IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<CycleCountDto> Handle(StartCycleCountCommand request, CancellationToken cancellationToken)
    {
        var startedBy = _currentUser.UserId ?? throw new InvalidOperationException("No authenticated user.");

        if (!await _repository.BinExistsAsync(request.CompanyId, request.ZoneId, request.BinId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.BinId), "Bin does not exist in this zone."),
            });
        }

        var cycleCount = Domain.CycleCounts.CycleCount.Start(
            request.CompanyId, request.WarehouseId, request.ZoneId, request.BinId, request.ProductId, request.SystemQuantitySnapshot, startedBy);

        await _repository.AddAsync(cycleCount, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Map(cycleCount);
    }

    internal static CycleCountDto Map(Domain.CycleCounts.CycleCount cycleCount) => new(
        cycleCount.Id,
        cycleCount.WarehouseId,
        cycleCount.ZoneId,
        cycleCount.BinId,
        cycleCount.ProductId,
        cycleCount.StartedBy,
        cycleCount.SystemQuantitySnapshot,
        cycleCount.CountedQuantity,
        cycleCount.VarianceQuantity,
        cycleCount.Status.ToString(),
        cycleCount.CreatedAt);
}
