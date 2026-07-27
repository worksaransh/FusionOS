using FluentValidation.Results;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveBalances.Commands.AdjustLeaveBalance;

public sealed class AdjustLeaveBalanceCommandHandler : IRequestHandler<AdjustLeaveBalanceCommand, LeaveBalanceDto>
{
    private readonly ILeaveBalanceRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public AdjustLeaveBalanceCommandHandler(ILeaveBalanceRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveBalanceDto> Handle(AdjustLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        var balance = await _repository.GetByIdAsync(request.CompanyId, request.LeaveBalanceId, cancellationToken)
            ?? throw new KeyNotFoundException($"Leave balance '{request.LeaveBalanceId}' was not found.");

        // Domain-invariant violation (new entitlement below days already used) surfaced as a
        // validation error, same re-throw convention as UpdateRoleCommandHandler's
        // catch (InvalidOperationException) around Role.Rename().
        try
        {
            balance.AdjustEntitlement(request.NewEntitledDays);
        }
        catch (InvalidOperationException ex)
        {
            throw new ValidationException(new[] { new ValidationFailure(nameof(request.NewEntitledDays), ex.Message) });
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeaveBalanceMapper.ToDto(balance);
    }
}
