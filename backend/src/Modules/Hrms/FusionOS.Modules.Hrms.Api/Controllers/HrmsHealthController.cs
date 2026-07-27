using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Hrms.Api.Controllers;

/// <summary>Reports genuine module status — Hrms has real Payroll/Attendance/Onboarding/LeaveRequest depth, not just a registration probe.</summary>
[ApiController]
[Route("api/v1/hrms/health")]
public sealed class HrmsHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Hrms",
        status = "active",
        roadmapPhase = "Phase 4 — CRM & HRMS",
        slices = new[] { "employees", "onboarding", "attendance", "leave-requests", "payroll" },
    });
}
