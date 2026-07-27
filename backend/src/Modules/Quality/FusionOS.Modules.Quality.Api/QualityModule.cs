using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Quality.Application.Audits.Contracts;
using FusionOS.Modules.Quality.Application.Calibrations.Contracts;
using FusionOS.Modules.Quality.Application.Certificates.Contracts;
using FusionOS.Modules.Quality.Application.CorrectiveActions.Contracts;
using FusionOS.Modules.Quality.Application.Deviations.Contracts;
using FusionOS.Modules.Quality.Application.InspectionPlans.Contracts;
using FusionOS.Modules.Quality.Application.Inspections.Commands.CreateInspection;
using FusionOS.Modules.Quality.Application.Inspections.Contracts;
using FusionOS.Modules.Quality.Application.IntegrationEvents.Consumers;
using FusionOS.BuildingBlocks.Infrastructure.Persistence;
using FusionOS.Modules.Quality.Application.NonConformanceReports.Contracts;
using FusionOS.Modules.Quality.Application.QualityHolds.Contracts;
using FusionOS.Modules.Quality.Infrastructure.Persistence;
using FusionOS.Modules.Quality.Infrastructure.Repositories;
using FusionOS.SharedKernel.Events;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Quality.Api;

/// <summary>
/// Phase 5 — Quality. Registers the module's DbContext, Inspections/NonConformanceReports/
/// CorrectiveActions/QualityAudits/DeviationRequests CQRS, repositories, and the outbox
/// dispatcher that relays InspectionCompleted etc. to Kafka (03_SYSTEM_ARCHITECTURE.md §4.2).
/// Quality now also consumes cross-module events (2026-07-23) — see the three
/// IIntegrationEventConsumer registrations below — so it is no longer purely event-producing.
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
        services.AddScoped<IQualityAuditRepository, QualityAuditRepository>();
        services.AddScoped<IDeviationRequestRepository, DeviationRequestRepository>();
        services.AddScoped<IInspectionPlanRepository, InspectionPlanRepository>();
        services.AddScoped<IQualityHoldRepository, QualityHoldRepository>();
        services.AddScoped<ICalibrationRecordRepository, CalibrationRecordRepository>();
        services.AddScoped<IQualityCertificateRepository, QualityCertificateRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IProcessedIntegrationEventStore, EfProcessedIntegrationEventStore<QualityDbContext>>();

        services.AddModuleApplication(typeof(CreateInspectionCommand).Assembly);

        // Kafka consumer side of 03_SYSTEM_ARCHITECTURE.md §4.2 (2026-07-23) — Quality reacts
        // to Warehouse's goods receipts, Manufacturing's completed work orders, and Sales's
        // dispatches by auto-opening the corresponding Inspection record so none of the three
        // go unnoticed. Notification-only — see each consumer's own doc comment. Discovered
        // generically by KafkaConsumerHostedService at startup.
        services.AddScoped<IIntegrationEventConsumer, GoodsReceiptLineReceivedConsumer>();
        services.AddScoped<IIntegrationEventConsumer, WorkOrderCompletedConsumer>();
        services.AddScoped<IIntegrationEventConsumer, DispatchLineDispatchedConsumer>();

        services.AddControllers().AddApplicationPart(typeof(QualityModule).Assembly);

        services.AddHostedService<OutboxDispatcher<QualityDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
