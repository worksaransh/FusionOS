using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.EWayBills.Contracts;

namespace FusionOS.Modules.Sales.Application.EWayBills.Commands.GenerateEWayBill;

public sealed record GenerateEWayBillCommand(Guid CompanyId, Guid DispatchId)
    : ICommand<EWayBillDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.dispatch.generate-e-way-bill" };
    public string EntityType => nameof(Domain.EWayBills.EWayBill);
    public Guid EntityId => DispatchId;
    public string Action => "Generated";
}
