using FusionOS.Modules.Core.Application.AuditLog.Queries.ListAuditLogEntries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Core.Api.Controllers;

/// <summary>
/// Read side of the insert-only audit trail (Phase H4, 2026-07-14 sprint).
/// Every entry here was already being written by AuditBehavior/EfAuditLogWriter
/// for every IAuditableCommand — this is simply the first exposed way to read
/// them back, company-scoped and gated by "core.audit.read".
/// </summary>
[ApiController]
[Route("api/v1/core/audit-log")]
public sealed class AuditLogController : ControllerBase
{
    private readonly ISender _sender;

    public AuditLogController(ISender sender) => _sender = sender;

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListAuditLogEntriesQuery(companyId, search, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
