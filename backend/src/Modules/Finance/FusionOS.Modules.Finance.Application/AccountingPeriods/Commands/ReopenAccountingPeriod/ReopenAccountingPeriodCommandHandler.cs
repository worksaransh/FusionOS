using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using MediatR;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.ReopenAccountingPeriod;

public sealed class ReopenAccountingPeriodCommandHandler : IRequestHandler<ReopenAccountingPeriodCommand, AccountingPeriodDto>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReopenAccountingPeriodCommandHandler(IAccountingPeriodRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountingPeriodDto> Handle(ReopenAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var period = await _repository.GetByIdAsync(request.CompanyId, request.AccountingPeriodId, cancellationToken)
            ?? throw new KeyNotFoundException($"Accounting period '{request.AccountingPeriodId}' was not found.");

        period.Reopen();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountingPeriodMapper.ToDto(period);
    }
}
