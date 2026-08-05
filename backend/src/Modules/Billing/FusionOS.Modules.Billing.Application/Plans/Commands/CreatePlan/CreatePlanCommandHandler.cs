using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Billing.Application.Plans.Contracts;
using FusionOS.Modules.Billing.Domain.Plans;
using MediatR;

namespace FusionOS.Modules.Billing.Application.Plans.Commands.CreatePlan;

public sealed class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, PlanDto>
{
    private readonly IPlanRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlanCommandHandler(IPlanRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<PlanDto> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<PlanTier>(request.Tier, ignoreCase: true, out var tier))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Tier), $"Unknown plan tier '{request.Tier}'."),
            });
        }

        if (await _repository.CodeExistsAsync(request.Code, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.Code), $"Plan code '{request.Code}' already exists."),
            });
        }

        var plan = Plan.Create(request.Code, request.Name, tier, request.MonthlyPricePerUser, request.IncludedModules);

        await _repository.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(plan);
    }

    internal static PlanDto MapToDto(Plan plan) =>
        new(plan.Id, plan.Code, plan.Name, plan.Tier.ToString(), plan.MonthlyPricePerUser, plan.IncludedModules, plan.CreatedAt);
}
