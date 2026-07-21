using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using FusionOS.Modules.Quality.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Quality.Api;

/// <summary>
/// Phase 5 — Quality. Registers the module's DbContext, Inspections/NonConformanceReports/
/// CorrectiveActions CQRS, repositories, and the outbox dispatcher that relays
/// InspectionCompleted etc. to Kafka (03_SYSTEM_ARCHITECTURE.md §4.2). Quality publishes
/// events but consumes none, so no IIntegrationEventConsumer is registered here.
/// </summary>
public sealed class QualityModule : IModule
{
    public string Name => "Quality";
    public string RoadmapPhase => "Phase 5 — Quality & Maintenance";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<QualityDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "quality")));

        services.AddScoped<IInspectionRepository, InspectionRepository>();
        services.AddScoped<INonConformanceReportRepository, NonConformanceReportRepository>();
        services.AddScoped<ICorrectiveActionRepository, CorrectiveActionRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateInspectionCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(QualityModule).Assembly);

        services.AddHostedService<OutboxDispatcher<QualityDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
