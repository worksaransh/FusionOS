using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;

namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.CreateSupplierContract;

public sealed record CreateSupplierContractCommand(Guid CompanyId, Guid SupplierId, DateTimeOffset StartDate, DateTimeOffset EndDate, string Terms)
    : ICommand<SupplierContractDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.contract.create" };
    public string EntityType => nameof(Domain.SupplierContracts.SupplierContract);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
