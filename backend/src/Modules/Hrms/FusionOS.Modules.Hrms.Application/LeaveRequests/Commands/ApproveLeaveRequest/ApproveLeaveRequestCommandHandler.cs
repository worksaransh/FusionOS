using FluentValidation.Results;
using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveBalances.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using MediatR;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.ApproveLeaveRequest;

/// <summary>
/// Approving used to flip Status with zero check against any balance. This now looks up the
/// matching LeaveBalance for (companyId, leaveRequest.EmployeeId, leaveRequest.Type,
/// leaveRequest.StartDate.Year) and deducts the request's DurationInDays from it before
/// approving.
///
/// If no balance record exists for that employee/type/year, approval is rejected outright —
/// silently approving without a balance to check against would grant unlimited leave with no
/// warning, so this surfaces as an InvalidOperationException (same "domain-state violation"
/// convention as LeaveRequest.Approve()'s status guard and LeaveBalance.Deduct()/Restore()).
/// A LeaveBalance record must be created (see CreateLeaveBalanceCommandHandler) before any
/// leave request for that employee/type/year can be approved.
/// </summary>
public sealed class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, LeaveRequestDto>
{
    private readonly ILeaveRequestRepository _repository;
    private readonly ILeaveBalanceRepository _leaveBalanceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveLeaveRequestCommandHandler(ILeaveRequestRepository repository, ILeaveBalanceRepository leaveBalanceRepository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _leaveBalanceRepository = leaveBalanceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LeaveRequestDto> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _repository.GetByIdAsync(request.CompanyId, request.LeaveRequestId, cancellationToken)
            ?? throw new KeyNotFoundException($"Leave request '{request.LeaveRequestId}' was not found.");

        var balance = await _leaveBalanceRepository.GetAsync(
            request.CompanyId, leaveRequest.EmployeeId, leaveRequest.Type, leaveRequest.StartDate.Year, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Cannot approve: no {leaveRequest.StartDate.Year} {leaveRequest.Type} leave balance found for employee '{leaveRequest.EmployeeId}'.");

        // Domain-invariant violation (would exceed the remaining balance) surfaced as a
        // validation error, same re-throw convention as UpdateRoleCommandHandler's
        // catch (InvalidOperationException) around Role.Rename().
        try
        {
            balance.Deduct(leaveRequest.DurationInDays);
        }
        catch (InvalidOperationException)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(
                    nameof(request.LeaveRequestId),
                    $"Insufficient leave balance: {balance.RemainingDays} day(s) remaining, {leaveRequest.DurationInDays} requested."),
            });
        }

        leaveRequest.Approve();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return LeaveRequestMapper.ToDto(leaveRequest);
    }
}
