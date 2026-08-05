using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.SodConflictRules.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.SodConflictRules.Commands.CreateSodConflictRule;

public sealed class CreateSodConflictRuleCommandHandler : IRequestHandler<CreateSodConflictRuleCommand, SodConflictRuleDto>
{
    private readonly ISodConflictRuleRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSodConflictRuleCommandHandler(ISodConflictRuleRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<SodConflictRuleDto> Handle(CreateSodConflictRuleCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ConflictExistsAsync(request.CompanyId, request.ConflictingPermissionA, request.ConflictingPermissionB, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.ConflictingPermissionB), $"A conflict rule already exists for '{request.ConflictingPermissionA}' and '{request.ConflictingPermissionB}' for this company."),
            });
        }

        var rule = Domain.SodConflictRules.SodConflictRule.Create(request.CompanyId, request.ConflictingPermissionA, request.ConflictingPermissionB, request.Description);

        await _repository.AddAsync(rule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(rule);
    }

    internal static SodConflictRuleDto MapToDto(Domain.SodConflictRules.SodConflictRule rule) =>
        new(rule.Id, rule.ConflictingPermissionA, rule.ConflictingPermissionB, rule.Description, rule.IsActive);
}
