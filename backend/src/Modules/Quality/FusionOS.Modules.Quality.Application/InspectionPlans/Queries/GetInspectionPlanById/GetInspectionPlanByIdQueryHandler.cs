using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using MediatR;

namespace FusionOS.Modules.Quality.Application.InspectionPlans.Queries.GetInspectionPlanById;

public sealed class GetInspectionPlanByIdQueryHandler : IRequestHandler<GetInspectionPlanByIdQuery, InspectionPlanDto>
{
    private readonly IInspectionPlanRepository _repository;

    public GetInspectionPlanByIdQueryHandler(IInspectionPlanRepository repository) => _repository = repository;

    public async Task<InspectionPlanDto> Handle(GetInspectionPlanByIdQuery request, CancellationToken cancellationToken)
    {
        var plan = await _repository.GetByIdAsync(request.CompanyId, request.InspectionPlanId, cancellationToken)
            ?? throw new KeyNotFoundException($"Inspection plan '{request.InspectionPlanId}' was not found.");

        return InspectionPlanMapper.ToDto(plan);
    }
}
