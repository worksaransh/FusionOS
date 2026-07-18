using FusionOS.Modules.Warehouse.Application.CycleCounts.Commands.StartCycleCount;
using FusionOS.Modules.Warehouse.Application.CycleCounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Warehouse.Application.CycleCounts.Queries.GetCycleCountById;

public sealed class GetCycleCountByIdQueryHandler : IRequestHandler<GetCycleCountByIdQuery, CycleCountDto?>
{
    private readonly ICycleCountRepository _repository;

    public GetCycleCountByIdQueryHandler(ICycleCountRepository repository) => _repository = repository;

    public async Task<CycleCountDto?> Handle(GetCycleCountByIdQuery request, CancellationToken cancellationToken)
    {
        var cycleCount = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (cycleCount is null || cycleCount.CompanyId != request.CompanyId)
            return null;

        return StartCycleCountCommandHandler.Map(cycleCount);
    }
}
