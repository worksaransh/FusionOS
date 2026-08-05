using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.CreateIntercompanyEliminationRule;
using FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.IntercompanyEliminationRules.Commands.DeactivateIntercompanyEliminationRule;

public sealed class DeactivateIntercompanyEliminationRuleCommandHandler : IRequestHandler<DeactivateIntercompanyEliminationRuleCommand, IntercompanyEliminationRuleDto>
{
    private readonly IIntercompanyEliminationRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateIntercompanyEliminationRuleCommandHandler(IIntercompanyEliminationRuleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IntercompanyEliminationRuleDto> Handle(DeactivateIntercompanyEliminationRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.CompanyId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Intercompany elimination rule '{request.Id}' was not found.");

        rule.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateIntercompanyEliminationRuleCommandHandler.MapToDto(rule);
    }
}
