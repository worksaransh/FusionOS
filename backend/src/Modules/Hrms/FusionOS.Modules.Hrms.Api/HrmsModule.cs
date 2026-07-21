using FusionOS.BuildingBlocks.Application;
using FusionOS.BuildingBlocks.Application.Modularity;
using FusionOS.BuildingBlocks.EventBus;
using FusionOS.Modules.Hrms.Application.Attendance.Contracts;
using FusionOS.Modules.Hrms.Application.Employees.Commands.CreateEmployee;
using FusionOS.Modules.Hrms.Application.Employees.Contracts;
using FusionOS.Modules.Hrms.Application.LeaveRequests.Contracts;
using FusionOS.Modules.Hrms.Application.Payroll.Contracts;
using FusionOS.Modules.Hrms.Infrastructure.Persistence;
using FusionOS.Modules.Hrms.Infrastructure.Repositories;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace FusionOS.Modules.Hrms.Api;

/// <summary>
/// Phase 4 — HRMS. Registers the module's DbContext, Employees + LeaveRequests
/// + AttendanceRecords + PayrollRecords CQRS, repositories, and the outbox
/// dispatcher that relays EmployeeCreated/LeaveRequestCreated/
/// LeaveRequestApproved/AttendanceRecorded/PayrollRecordDrafted to Kafka
/// (03_SYSTEM_ARCHITECTURE.md §4.2). HRMS publishes events but consumes none,
/// so no IIntegrationEventConsumer is registered here.
/// </summary>
public sealed class HrmsModule : IModule
{
    public string Name => "Hrms";
    public string RoadmapPhase => "Phase 4 — CRM & HRMS";

    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<HrmsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Postgres"),
                npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", "hrms")));

        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<ILeaveRequestRepository, LeaveRequestRepository>();
        services.AddScoped<IAttendanceRecordRepository, AttendanceRecordRepository>();
        services.AddScoped<IPayrollRecordRepository, PayrollRecordRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddModuleApplication(typeof(CreateEmployeeCommand).Assembly);

        services.AddControllers().AddApplicationPart(typeof(HrmsModule).Assembly);

        services.AddHostedService<OutboxDispatcher<HrmsDbContext>>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Controllers are attribute-routed and mapped once, globally, via
        // endpoints.MapControllers() in the Host.
    }
}
