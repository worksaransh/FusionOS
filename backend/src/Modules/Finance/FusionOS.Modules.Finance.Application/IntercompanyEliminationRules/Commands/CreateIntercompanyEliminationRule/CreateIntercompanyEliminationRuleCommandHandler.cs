using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.CreateIntercompanyEliminationRule;

public sealed class CreateIntercompanyEliminationRuleCommandHandler : IRequestHandler<CreateIntercompanyEliminationRuleCommand, IntercompanyEliminationRuleDto>
{
    private readonly IIntercompanyEliminationRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateIntercompanyEliminationRuleCommandHandler(IIntercompanyEliminationRuleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IntercompanyEliminationRuleDto> Handle(CreateIntercompanyEliminationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = Domain.IntercompanyEliminationRules.IntercompanyEliminationRule.Create(
            request.CompanyId, request.ReceivableAccountCode, request.PayableAccountCode, request.Description, request.BranchId);

        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rule);
    }

    internal static IntercompanyEliminationRuleDto MapToDto(Domain.IntercompanyEliminationRules.IntercompanyEliminationRule rule) =>
        new(rule.Id, rule.ReceivableAccountCode, rule.PayableAccountCode, rule.Description, rule.IsActive);
}
