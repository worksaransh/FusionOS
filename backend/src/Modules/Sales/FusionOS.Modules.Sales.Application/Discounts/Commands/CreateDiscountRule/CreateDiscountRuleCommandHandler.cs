using FusionOS.Modules.Sales.Application.Customers.Contracts;
using FusionOS.Modules.Sales.Application.Discounts.Contracts;
using MediatR;

namespace FusionOS.Modules.Sales.Application.Discounts.Commands.CreateDiscountRule;

public sealed class CreateDiscountRuleCommandHandler : IRequestHandler<CreateDiscountRuleCommand, DiscountRuleDto>
{
    private readonly IDiscountRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateDiscountRuleCommandHandler(IDiscountRuleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DiscountRuleDto> Handle(CreateDiscountRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = Domain.Discounts.DiscountRule.Create(request.CompanyId, request.ProductId, request.MinQuantity, request.DiscountPercentage);

        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return DiscountRuleMapper.ToDto(rule);
    }
}
