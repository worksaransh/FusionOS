using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Commissions.Contracts;

namespace FusionOS.Modules.Sales.Application.Commissions.Commands.SetCommissionRate;

public sealed record SetCommissionRateCommand(Guid CompanyId, Guid UserId, decimal RatePercentage)
    : ICommand<SalesCommissionRateDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.commission-rate.set" };
    public string EntityType => nameof(Domain.Commissions.SalesCommissionRate);
    public Guid EntityId { get; init; }
    public string Action => "Set";
}
