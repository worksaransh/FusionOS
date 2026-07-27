using FluentValidation;

namespace FusionOS.Modules.Quality.Application.Calibrations.Commands.CreateCalibrationRecord;

public sealed class CreateCalibrationRecordValidator : AbstractValidator<CreateCalibrationRecordCommand>
{
    public CreateCalibrationRecordValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.EquipmentReferenceId).NotEmpty();
        RuleFor(x => x.EquipmentName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EquipmentIdentifier).MaximumLength(100);
        RuleFor(x => x.CalibrationStandard).MaximumLength(100);
        RuleFor(x => x.PerformedByExternalVendor).MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.NextCalibrationDueDate)
            .GreaterThanOrEqualTo(x => x.LastCalibratedDate)
            .WithMessage("Next calibration due date cannot be before the last calibrated date.");
    }
}
