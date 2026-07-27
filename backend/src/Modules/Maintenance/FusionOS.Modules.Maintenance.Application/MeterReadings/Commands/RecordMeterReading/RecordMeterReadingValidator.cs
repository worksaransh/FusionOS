using FluentValidation;

namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Commands.RecordMeterReading;

public sealed class RecordMeterReadingValidator : AbstractValidator<RecordMeterReadingCommand>
{
    public RecordMeterReadingValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.AssetId).NotEmpty();
        RuleFor(x => x.ReadingValue).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Unit).NotEmpty().MaximumLength(50);
    }
}
