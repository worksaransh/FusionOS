using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Sales.Application.PriceLists.Contracts;
using FusionOS.Modules.Sales.Domain.PriceLists;

namespace FusionOS.Modules.Sales.Application.PriceLists.Commands.CreatePriceList;

public sealed record CreatePriceListCommand(Guid CompanyId, string Name, IReadOnlyList<PriceListEntryInput> Entries)
    : ICommand<PriceListDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "sales.price-list.create" };
    public string EntityType => nameof(Domain.PriceLists.PriceList);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
