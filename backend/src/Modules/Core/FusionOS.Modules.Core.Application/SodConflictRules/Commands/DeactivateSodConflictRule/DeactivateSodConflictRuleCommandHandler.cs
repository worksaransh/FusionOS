using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.SodConflictRules.Commands.CreateSodConflictRule;
using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Commands.DeactivateSodConflictRule;

public sealed class DeactivateSodConflictRuleCommandHandler : IRequestHandler<DeactivateSodConflictRuleCommand, SodConflictRuleDto>
{
    private readonly ISodConflictRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateSodConflictRuleCommandHandler(ISodConflictRuleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SodConflictRuleDto> Handle(DeactivateSodConflictRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _repository.GetByIdAsync(request.CompanyId, request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"SoD conflict rule '{request.Id}' was not found.");

        rule.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return CreateSodConflictRuleCommandHandler.MapToDto(rule);
    }
}
