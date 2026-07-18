using FusionOS.Modules.Finance.Application.CostCenters.Commands.CreateCostCenter;
using FusionOS.Modules.Finance.Application.CostCenters.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.CostCenters.Queries.GetCostCenterById;

public sealed class GetCostCenterByIdQueryHandler : IRequestHandler<GetCostCenterByIdQuery, CostCenterDto>
{
    private readonly ICostCenterRepository _repository;

    public GetCostCenterByIdQueryHandler(ICostCenterRepository repository) => _repository = repository;

    public async Task<CostCenterDto> Handle(GetCostCenterByIdQuery request, CancellationToken cancellationToken)
    {
        var costCenter = await _repository.GetByIdAsync(request.CompanyId, request.CostCenterId, cancellationToken)
            ?? throw new KeyNotFoundException($"Cost center '{request.CostCenterId}' was not found.");

        return CreateCostCenterCommandHandler.MapToDto(costCenter);
    }
}
