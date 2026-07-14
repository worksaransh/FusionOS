using FusionOS.BuildingBlocks.Application.Modularity;

namespace FusionOS.Api.Host.Modularity;

/// <summary>
/// Central list of every FusionOS module. Adding a new module (e.g. when Phase 1
/// begins real implementation) means adding one line here — nothing else in the
/// Host changes, per the modular-monolith contract in 03_SYSTEM_ARCHITECTURE.md.
/// </summary>
public static class ModuleRegistry
{
    public static IReadOnlyList<IModule> All { get; } = new List<IModule>
    {
        new FusionOS.Modules.Core.Api.CoreModule(),
        new FusionOS.Modules.Inventory.Api.InventoryModule(),
        new FusionOS.Modules.Warehouse.Api.WarehouseModule(),
        new FusionOS.Modules.Procurement.Api.ProcurementModule(),
        new FusionOS.Modules.Sales.Api.SalesModule(),
        new FusionOS.Modules.Finance.Api.FinanceModule(),
        new FusionOS.Modules.Manufacturing.Api.ManufacturingModule(),
        new FusionOS.Modules.Crm.Api.CrmModule(),
        new FusionOS.Modules.Hrms.Api.HrmsModule(),
        new FusionOS.Modules.Quality.Api.QualityModule(),
        new FusionOS.Modules.Maintenance.Api.MaintenanceModule(),
        new FusionOS.Modules.BusinessIntelligence.Api.BusinessIntelligenceModule(),
        new FusionOS.Modules.Ai.Api.AiModule(),
        new FusionOS.Modules.Marketplace.Api.MarketplaceModule(),
        new FusionOS.Modules.IntegrationHub.Api.IntegrationHubModule(),
    };
}
