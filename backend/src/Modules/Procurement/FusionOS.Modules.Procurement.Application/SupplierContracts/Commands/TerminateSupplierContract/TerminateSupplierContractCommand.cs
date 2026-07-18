using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Procurement.Application.SupplierContracts.Contracts;

namespace FusionOS.Modules.Procurement.Application.SupplierContracts.Commands.TerminateSupplierContract;

public sealed record TerminateSupplierContractCommand(Guid CompanyId, Guid SupplierContractId)
    : ICommand<SupplierContractDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "procurement.contract.terminate" };
    public string EntityType => nameof(Domain.SupplierContracts.SupplierContract);
    public Guid EntityId => SupplierContractId;
    public string Action => "Terminated";
}
