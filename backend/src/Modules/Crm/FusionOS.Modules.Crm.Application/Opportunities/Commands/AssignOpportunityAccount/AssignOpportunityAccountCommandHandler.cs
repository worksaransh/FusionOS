using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Opportunities.Commands.AssignOpportunityAccount;

public sealed class AssignOpportunityAccountCommandHandler : IRequestHandler<AssignOpportunityAccountCommand, OpportunityDto>
{
    private readonly IOpportunityRepository _opportunityRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignOpportunityAccountCommandHandler(IOpportunityRepository opportunityRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _opportunityRepository = opportunityRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<OpportunityDto> Handle(AssignOpportunityAccountCommand request, CancellationToken cancellationToken)
    {
        var opportunity = await _opportunityRepository.GetByIdAsync(request.CompanyId, request.OpportunityId, cancellationToken)
            ?? throw new KeyNotFoundException($"Opportunity '{request.OpportunityId}' was not found.");

        if (request.AccountId is { } accountId &&
            !await _accountRepository.ExistsAsync(request.CompanyId, accountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AccountId), "Account does not exist for this company."),
            });
        }

        opportunity.AssignAccount(request.AccountId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return OpportunityMapper.ToDto(opportunity);
    }
}
