using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Marketplace.Application.PluginInstallations.Contracts;
using FusionOS.Modules.Marketplace.Application.PluginListings.Commands.CreatePluginListing;
using FusionOS.Modules.Marketplace.Application.PluginListings.Contracts;
using FusionOS.Modules.Marketplace.Infrastructure.Persistence;
using FusionOS.Modules.Marketplace.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Marketplace.Api;

/// <summary>
/// Phase 8 — Marketplace, first slice (2026-07-18). Registers the module's DbContext,
/// PluginListings + PluginInstallations CQRS, repositories, and the outbox dispatcher
/// that relays PluginListingCreated/PluginInstalled to Kafka (03_SYSTEM_ARCHITECTURE.md
/// §4.2). Marketplace publishes events but consumes none this slice, so no
/// IIntegrationEventConsumer is registered here.
/// </summary>
public sealed class MarketplaceModule : IModule
{
    public string Name => "Marketplace";
    public string RoadmapPhase => "Phase 8 — Marketplace & Ecosystem";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MarketplaceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "marketplace")));

        services.AddScoped<IPluginListingRepository, PluginListingRepository>();
        services.AddScoped<IPluginInstallationRepository, PluginInstallationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreatePluginListingCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(MarketplaceModule).Assembly);

        services.AddHostedService<OutboxDispatcher<MarketplaceDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
