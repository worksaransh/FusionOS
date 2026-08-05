using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.Plans.Commands.CreatePlan;
using FusionOS.Modules.Billing.Application.Plans.Contracts;
using MediatR;

namespace FusionOS.Modules.Billing.Application.Plans.Queries.ListPlans;

public sealed class ListPlansQueryHandler : IRequestHandler<ListPlansQuery, PagedResult<PlanDto>>
{
    private readonly IPlanRepository _repository;

    public ListPlansQueryHandler(IPlanRepository repository) => _repository = repository;

    public async Task<PagedResult<PlanDto>> Handle(ListPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _repository.ListAsync(request.Page, request.PageSize, cancellationToken);
        var total = await _repository.CountAsync(cancellationToken);

        return new PagedResult<PlanDto>(plans.Select(CreatePlanCommandHandler.MapToDto).ToList(), request.Page, request.PageSize, total);
    }
}
