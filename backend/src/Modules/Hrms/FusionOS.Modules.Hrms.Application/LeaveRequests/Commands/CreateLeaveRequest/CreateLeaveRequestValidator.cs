using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.LeaveRequests.Commands.CreateLeaveRequest;

public sealed class CreateLeaveRequestValidator : AbstractValidator<CreateLeaveRequestCommand>
{
    public CreateLeaveRequestValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.EndDate).GreaterThanOrEqualTo(x => x.StartDate).WithMessage("End date cannot be before start date.");
        RuleFor(x => x.Reason).MaximumLength(1000);
    }
}
