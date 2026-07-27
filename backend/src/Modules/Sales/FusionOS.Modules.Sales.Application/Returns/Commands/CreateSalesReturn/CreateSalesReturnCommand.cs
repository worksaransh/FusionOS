using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using FusionOS.Modules.Sales.Domain.Returns;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.CreateSalesReturn;

public sealed record CreateSalesReturnCommand(Guid CompanyId, Guid InvoiceId, Guid CustomerId, string Reason, IReadOnlyList<SalesReturnLineInput> Lines)
    : ICommand<SalesReturnDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.return.create" };
    public string EntityType => nameof(Domain.Returns.SalesReturn);
    public Guid EntityId { get; init; }
    public string Action => "Requested";
}
