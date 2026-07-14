using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Core.Application.Companies.Contracts;

namespace FusionOS.Modules.Core.Application.Companies.Commands.CreateCompany;

/// <summary>
/// The one fully-implemented vertical slice for Phase 0 (05_MODULE_ROADMAP.md).
/// Company creation does not require a permission check — it is the bootstrap
/// action of a fresh tenant — but it IS audited.
/// </summary>
public sealed record CreateCompanyCommand(string Name, string LegalName, string BaseCurrency, string? TaxId)
    : ICommand<CompanyDto>, IAuditableCommand
{
    public string EntityType => nameof(Domain.Companies.Company);
    public Guid EntityId { get; init; }
    public string Action => "Created";
}
