using FluentValidation;

namespace FusionOS.Modules.Hrms.Application.Attendance.Commands.UpdateAttendance;

public sealed class UpdateAttendanceValidator : AbstractValidator<UpdateAttendanceCommand>
{
    public UpdateAttendanceValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AttendanceRecordId).NotEmpty();
        RuleFor(x => x)
            .Must(x => !x.CheckInTime.HasValue || !x.CheckOutTime.HasValue || x.CheckOutTime.Value >= x.CheckInTime.Value)
            .WithName(nameof(UpdateAttendanceCommand.CheckOutTime))
            .WithMessage("Check-out time cannot be before check-in time.");
    }
}
