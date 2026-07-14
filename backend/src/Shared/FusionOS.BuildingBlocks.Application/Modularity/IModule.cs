using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.BuildingBlocks.Application.Modularity;

/// <summary>
/// Contract every FusionOS module implements. The Host composition root discovers
/// and registers one IModule per module (03_SYSTEM_ARCHITECTURE.md) — modules never
/// register themselves into another module's DI container or route group.
/// </summary>
public interface IModule
{
    /// <summary>Stable module name, e.g. "Core", "Inventory" — used for logging, schema naming, and route prefixes.</summary>
    string Name { get; }

    /// <summary>Roadmap phase this module belongs to, per 05_MODULE_ROADMAP.md (informational).</summary>
    string RoadmapPhase { get; }

    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
