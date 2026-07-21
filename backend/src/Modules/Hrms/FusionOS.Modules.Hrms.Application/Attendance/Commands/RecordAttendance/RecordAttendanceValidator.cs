using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Attendance.Commands.RecordAttendance;

public sealed class RecordAttendanceValidator : AbstractValidator<RecordAttendanceCommand>
{
    public RecordAttendanceValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x)
            .Must(x => !x.CheckInTime.HasValue || !x.CheckOutTime.HasValue || x.CheckOutTime.Value >= x.CheckInTime.Value)
            .WithName(nameof(RecordAttendanceCommand.CheckOutTime))
            .WithMessage("Check-out time cannot be before check-in time.");
    }
}
