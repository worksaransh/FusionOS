using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Finance.Application.Accounts.Contracts;
using FusionOS.Modules.Finance.Application.AccountingPeriods.Contracts;
using FusionOS.Modules.Finance.Domain.AccountingPeriods;
using MediatR;

namespace FusionOS.Modules.Finance.Application.AccountingPeriods.Commands.CreateAccountingPeriod;

/// <summary>
/// Rejects a candidate [PeriodStart, PeriodEnd] range that overlaps an existing period for the
/// same company — AccountingPeriod.OverlapsWith is a pure range comparison, so every existing
/// period for the company is loaded once (ListAllAsync) and checked in memory, the same
/// "load-then-check" split CreateAccountCommandHandler/CreateCostCenterCommandHandler use for
/// their own uniqueness checks (there just via a dedicated CodeExistsAsync instead).
/// </summary>
public sealed class CreateAccountingPeriodCommandHandler : IRequestHandler<CreateAccountingPeriodCommand, AccountingPeriodDto>
{
    private readonly IAccountingPeriodRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAccountingPeriodCommandHandler(IAccountingPeriodRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AccountingPeriodDto> Handle(CreateAccountingPeriodCommand request, CancellationToken cancellationToken)
    {
        var existingPeriods = await _repository.ListAllAsync(request.CompanyId, cancellationToken);
        var overlapping = existingPeriods.FirstOrDefault(p => p.OverlapsWith(request.PeriodStart, request.PeriodEnd));
        if (overlapping is not null)
        {
            throw new ValidationException(new[]
            {
                new FluentValidation.Results.ValidationFailure(nameof(request.PeriodStart),
                    $"This period overlaps an existing period ({overlapping.PeriodStart:yyyy-MM-dd} to {overlapping.PeriodEnd:yyyy-MM-dd}) for this company."),
            });
        }

        var period = AccountingPeriod.Create(request.CompanyId, request.PeriodStart, request.PeriodEnd);

        await _repository.AddAsync(period, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return AccountingPeriodMapper.ToDto(period);
    }
}
