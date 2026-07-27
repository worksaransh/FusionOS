using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.Returns.Contracts;
using FusionOS.Modules.Sales.Domain.Returns;

namespace FusionOS.Modules.Sales.Application.Returns.Commands.CompleteSalesReturn;

public sealed record CompleteSalesReturnCommand(Guid CompanyId, Guid SalesReturnId, IReadOnlyList<SalesReturnLineCompletion> LineCompletions)
    : ICommand<SalesReturnDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.return.complete" };
    public string EntityType => nameof(Domain.Returns.SalesReturn);
    public Guid EntityId => SalesReturnId;
    public string Action => "Completed";
}
