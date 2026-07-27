using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.IntegrationHub.Application.ConnectorConnections.Contracts;
using FusionOS.Modules.IntegrationHub.Application.ConnectorSyncRuns.Contracts;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Commands.CreateIntegrationConnector;
using FusionOS.Modules.IntegrationHub.Application.IntegrationConnectors.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Contracts;
using FusionOS.Modules.IntegrationHub.Application.Sync;
using FusionOS.Modules.IntegrationHub.Infrastructure.Persistence;
using FusionOS.Modules.IntegrationHub.Infrastructure.Repositories;
using FusionOS.Modules.IntegrationHub.Infrastructure.Sync;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.IntegrationHub.Api;

/// <summary>
/// Phase 9 — Integration Hub, first slice (2026-07-18). Registers the module's DbContext,
/// IntegrationConnectors + ConnectorConnections CQRS, repositories, and the outbox
/// dispatcher that relays IntegrationConnectorCreated/ConnectorConnected to Kafka
/// (03_SYSTEM_ARCHITECTURE.md §4.2). IntegrationHub publishes events but consumes none
/// this slice, so no IIntegrationEventConsumer is registered here.
/// </summary>
public sealed class IntegrationHubModule : IModule
{
    public string Name => "IntegrationHub";
    public string RoadmapPhase => "Phase 9 — Integrations & Mobile";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IntegrationHubDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "integration_hub")));

        services.AddScoped<IIntegrationConnectorRepository, IntegrationConnectorRepository>();
        services.AddScoped<IConnectorConnectionRepository, ConnectorConnectionRepository>();
        services.AddScoped<IConnectorSyncRunRepository, ConnectorSyncRunRepository>();
        services.AddScoped<IMigrationJobRepository, MigrationJobRepository>();
        services.AddScoped<IMigrationStagingRecordRepository, MigrationStagingRecordRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateIntegrationConnectorCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(IntegrationHubModule).Assembly);

        // Phase 8 — AI & Integrations: provider-specific connector sync adapters
        // (payment/shipping). Each is registered as a typed HttpClient (standard
        // IHttpClientFactory pattern) plus its IConnectorSyncAdapter registration.
        // AddHttpClient<T>() registers T itself (transient) constructed via the
        // typed-client factory with its HttpClient pre-wired; the IConnectorSyncAdapter
        // registrations below resolve that same instance rather than re-activating T
        // directly (which would fail to satisfy T's HttpClient constructor parameter).
        services.AddHttpClient<RazorpayConnectorSyncAdapter>();
        services.AddHttpClient<StripeConnectorSyncAdapter>();
        services.AddHttpClient<ShiprocketConnectorSyncAdapter>();
        services.AddHttpClient<DelhiveryConnectorSyncAdapter>();
        services.AddHttpClient<ShopifyConnectorSyncAdapter>();
        services.AddScoped<IConnectorSyncAdapter>(sp => sp.GetRequiredService<RazorpayConnectorSyncAdapter>());
        services.AddScoped<IConnectorSyncAdapter>(sp => sp.GetRequiredService<StripeConnectorSyncAdapter>());
        services.AddScoped<IConnectorSyncAdapter>(sp => sp.GetRequiredService<ShiprocketConnectorSyncAdapter>());
        services.AddScoped<IConnectorSyncAdapter>(sp => sp.GetRequiredService<DelhiveryConnectorSyncAdapter>());
        services.AddScoped<IConnectorSyncAdapter>(sp => sp.GetRequiredService<ShopifyConnectorSyncAdapter>());
        // Ondc needs no HttpClient — it's a structural placeholder, never makes an HTTP call (see its own class doc comment).
        services.AddScoped<IConnectorSyncAdapter, OndcConnectorSyncAdapter>();

        services.AddHostedService<OutboxDispatcher<IntegrationHubDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
