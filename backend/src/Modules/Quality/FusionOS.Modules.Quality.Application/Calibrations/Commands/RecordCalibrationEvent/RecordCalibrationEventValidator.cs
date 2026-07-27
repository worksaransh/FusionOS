using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Calibrations.Commands.RecordCalibrationEvent;

public sealed class RecordCalibrationEventValidator : AbstractValidator<RecordCalibrationEventCommand>
{
    public RecordCalibrationEventValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.CalibrationRecordId).NotEmpty();
        RuleFor(x => x.PerformedByExternalVendor).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.NextDueDate)
            .GreaterThanOrEqualTo(x => x.CalibratedDate)
            .WithMessage("Next calibration due date cannot be before the calibrated date.");
    }
}
