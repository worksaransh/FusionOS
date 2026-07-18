using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CompleteVendorReturn;

public sealed record CompleteVendorReturnCommand(Guid CompanyId, Guid VendorReturnId)
    : ICommand<VendorReturnDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.vendor-return.complete" };
    public string EntityType => nameof(Domain.VendorReturns.VendorReturn);
    public Guid EntityId => VendorReturnId;
    public string Action => "Completed";
}
