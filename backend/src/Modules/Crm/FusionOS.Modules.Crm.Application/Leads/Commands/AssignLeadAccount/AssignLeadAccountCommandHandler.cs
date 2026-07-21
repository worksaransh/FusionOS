using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Crm.Application.Accounts.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using MediatR;

namespace FusionOS.Modules.Crm.Application.Leads.Commands.AssignLeadAccount;

public sealed class AssignLeadAccountCommandHandler : IRequestHandler<AssignLeadAccountCommand, LeadDto>
{
    private readonly ILeadRepository _leadRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignLeadAccountCommandHandler(ILeadRepository leadRepository, IAccountRepository accountRepository, IUnitOfWork unitOfWork)
    {
        _leadRepository = leadRepository;
        _accountRepository = accountRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeadDto> Handle(AssignLeadAccountCommand request, CancellationToken cancellationToken)
    {
        var lead = await _leadRepository.GetByIdAsync(request.CompanyId, request.LeadId, cancellationToken)
            ?? throw new KeyNotFoundException($"Lead '{request.LeadId}' was not found.");

        if (request.AccountId is { } accountId &&
            !await _accountRepository.ExistsAsync(request.CompanyId, accountId, cancellationToken))
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.AccountId), "Account does not exist for this company."),
            });
        }

        lead.AssignAccount(request.AccountId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeadMapper.ToDto(lead);
    }
}
