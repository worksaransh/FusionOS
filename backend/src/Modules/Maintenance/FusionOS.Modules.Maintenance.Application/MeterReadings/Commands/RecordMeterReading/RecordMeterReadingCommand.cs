using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Maintenance.Application.MeterReadings.Contracts;

namespace FusionOS.Modules.Maintenance.Application.MeterReadings.Commands.RecordMeterReading;

public sealed record RecordMeterReadingCommand(Guid CompanyId, Guid AssetId, decimal ReadingValue, DateTimeOffset ReadingDate, string Unit)
    : ICommand<MeterReadingDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "maintenance.meter-reading.record" };
    public string EntityType => nameof(Domain.MeterReadings.MeterReading);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
