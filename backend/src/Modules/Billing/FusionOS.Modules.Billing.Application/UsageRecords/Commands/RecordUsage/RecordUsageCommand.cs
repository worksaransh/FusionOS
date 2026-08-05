using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Billing.Application.UsageRecords.Contracts;

namespace FusionOS.Modules.Billing.Application.UsageRecords.Commands.RecordUsage;

public sealed record RecordUsageCommand(Guid CompanyId, Guid SubscriptionId, string MetricType, int Quantity)
    : ICommand<UsageRecordDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "billing.usage-record.record" };
    public string EntityType => nameof(Domain.UsageRecords.UsageRecord);
    public Guid EntityId { get; init; } = Guid.NewGuid();
    public string Action => "Recorded";
}
