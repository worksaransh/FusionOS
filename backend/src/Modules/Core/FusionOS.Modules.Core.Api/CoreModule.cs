using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Core.Application.AuditLog.Contracts;
using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Commands.CreateCompany;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Contracts;
using FusionOS.Modules.Core.Application.Notifications.Services;
using FusionOS.Modules.Core.Application.Settings.Contracts;
using FusionOS.Modules.Core.Application.Workflow.Contracts;
using FusionOS.Modules.Core.Infrastructure.Auditing;
using FusionOS.Modules.Core.Infrastructure.BackgroundServices;
using FusionOS.Modules.Core.Infrastructure.Email;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using FusionOS.Modules.Core.Infrastructure.Repositories;
using FusionOS.Modules.Core.Infrastructure.Security;
using FusionOS.SharedKernel.Auditing;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Core.Api;

/// <summary>
/// The Core module's registration point. This is what the Host's ModuleRegistry
/// discovers and wires up (03_SYSTEM_ARCHITECTURE.md). Every future module
/// (Inventory, Warehouse, ...) follows exactly this shape.
/// </summary>
public sealed class CoreModule : IModule
{
    public string Name => "Core";
    public string RoadmapPhase => "Phase 0 — Platform Foundation";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<CoreDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "core")));

        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Settings module (Phase M5, 2026-07-15 — previously 0% per docs/PROJECT_TRACKER.md).
        services.AddScoped<ICompanySettingsRepository, CompanySettingsRepository>();

        // Generic Workflow/Approval engine + Notification read side (Phase M7, 2026-07-15).
        services.AddScoped<IApprovalRequestRepository, ApprovalRequestRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();

        // External notification delivery via SendGrid (Phase M7 remaining, 2026-07-16
        // — the notification-provider decision resolved to SendGrid). A blank
        // SendGrid:ApiKey degrades gracefully rather than failing startup — see
        // SendGridOptions' doc comment.
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SectionName));
        services.AddScoped<INotificationSender, SendGridNotificationSender>();
        services.AddScoped<NotificationDeliveryService>();
        services.AddHostedService<NotificationDeliveryDispatcher>();

        // Published platform-wide, per 04_DATABASE_GUIDELINES.md §5 — other modules
        // depend on IAuditLogWriter (SharedKernel), never on this concrete type.
        services.AddScoped<IAuditLogWriter, EfAuditLogWriter>();

        // Read side of the same audit trail (Phase H4, 2026-07-14 sprint).
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        // Auth (07_SECURITY.md) — real password hashing, JWT issuance, refresh-token
        // rotation. See Auth/Commands/{Login,Refresh,Register,Logout}.
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddModuleApplication(typeof(CreateCompanyCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(CoreModule).Assembly);

        services.AddHostedService<OutboxDispatcher<CoreDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers in this module use attribute routing and are mapped once,
        // globally, via endpoints.MapControllers() in the Host — nothing
        // module-specific to wire here today. Minimal-API modules (if any future
        // module prefers that style) would map their route group in this method.
    }
}
