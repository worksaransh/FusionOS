using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Finance.Application.TaxJurisdictions.Contracts;

namespace FusionOS.Modules.Finance.Application.TaxJurisdictions.Commands.CreateTaxJurisdiction;

public sealed record CreateTaxJurisdictionCommand(Guid CompanyId, string Code, string Name)
    : ICommand<TaxJurisdictionDto>, IRequirePermission, IAuditableCommand
{
    public string[] RequiredPermissions => new[] { "finance.tax-jurisdiction.create" };
    public string EntityType => nameof(Domain.TaxJurisdictions.TaxJurisdiction);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
