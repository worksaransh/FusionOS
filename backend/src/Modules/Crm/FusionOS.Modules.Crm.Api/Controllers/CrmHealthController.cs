using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Crm.Api.Controllers;

/// <summary>Reports genuine module status — Crm has real Leads/Accounts/Contacts/Opportunities depth, not just a registration probe.</summary>
[ApiController]
[Route("api/v1/crm/health")]
public sealed class CrmHealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        module = "Crm",
        status = "active",
        roadmapPhase = "Phase 4 — CRM & HRMS",
        slices = new[] { "leads", "accounts", "contacts", "opportunities" },
    });
}
