using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Discounts.Commands.DeactivateDiscountRule;

public sealed class DeactivateDiscountRuleCommandHandler : IRequestHandler<DeactivateDiscountRuleCommand, DiscountRuleDto>
{
    private readonly IDiscountRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateDiscountRuleCommandHandler(IDiscountRuleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DiscountRuleDto> Handle(DeactivateDiscountRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.CompanyId, request.DiscountRuleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Discount rule '{request.DiscountRuleId}' was not found.");

        rule.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DiscountRuleMapper.ToDto(rule);
    }
}
