using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Crm.Application.Leads.Commands.CreateLead;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Opportunities.Contracts;
using FusionOS.Modules.Crm.Infrastructure.Persistence;
using FusionOS.Modules.Crm.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Crm.Api;

/// <summary>
/// Phase 4 — CRM, first slice. Registers the module's DbContext, Leads + Opportunities
/// CQRS, repositories, and the outbox dispatcher that relays OpportunityWon to Kafka for
/// Sales to consume (03_SYSTEM_ARCHITECTURE.md §4.2). CRM publishes events but consumes
/// none, so no IIntegrationEventConsumer is registered here.
/// </summary>
public sealed class CrmModule : IModule
{
    public string Name => "Crm";
    public string RoadmapPhase => "Phase 4 — CRM & HRMS";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CrmDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "crm")));

        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddScoped<IOpportunityRepository, OpportunityRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateLeadCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(CrmModule).Assembly);

        services.AddHostedService<OutboxDispatcher<CrmDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
