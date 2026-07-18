using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.VendorReturns.Contracts;

namespace FusionOS.Modules.Procurement.Application.VendorReturns.Commands.CancelVendorReturn;

public sealed record CancelVendorReturnCommand(Guid CompanyId, Guid VendorReturnId)
    : ICommand<VendorReturnDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.vendor-return.cancel" };
    public string EntityType => nameof(Domain.VendorReturns.VendorReturn);
    public Guid EntityId => VendorReturnId;
    public string Action => "Cancelled";
}
