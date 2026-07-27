using FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.CreateMigrationJob;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ExtractTallyData;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.LoadMigrationStagingRecords;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ReconcileMigrationJob;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Commands.ValidateMigrationStagingRecords;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.GetMigrationJobById;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.ListMigrationJobs;
using FusionOS.Modules.IntegrationHub.Application.Migrations.Queries.ListMigrationStagingRecordsForJob;
using FusionOS.Modules.IntegrationHub.Domain.Migrations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.IntegrationHub.Api.Controllers;

/// <summary>
/// Phase 8 — the legacy-system migration pipeline (11_SAP_MIGRATION.md §2): Create a job,
/// run its Extract (Tally XML) → Validate → Load → Reconcile stages, and inspect its
/// staging records. Distinct from ConnectorConnectionsController's ongoing sync
/// bookkeeping — see MigrationJob's class doc comment for why these are not the same
/// concept.
/// </summary>
[ApiController]
[Route("api/v1/integration-hub/migrations")]
public sealed class MigrationJobsController : ControllerBase
{
    private readonly ISender _sender;

    public MigrationJobsController(ISender sender) => _sender = sender;

    /// <summary>Creates a new migration job for the given legacy source system, ready for its Extract stage.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateMigrationJobRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateMigrationJobCommand(request.CompanyId, request.SourceSystem), cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = request.CompanyId }, result);
    }

    /// <summary>Lists migration jobs for a company, paginated (default page size 25).</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMigrationJobsQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Gets a single migration job by id, scoped to the given company.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetMigrationJobByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Lists this job's staging records, optionally filtered by mapping status, paginated (default page size 50).</summary>
    [HttpGet("{id:guid}/staging-records")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListStagingRecords(Guid id, [FromQuery] Guid companyId, [FromQuery] MappingStatus? mappingStatus, [FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMigrationStagingRecordsForJobQuery(companyId, id, mappingStatus, page, pageSize), cancellationToken);
        return Ok(result);
    }

    /// <summary>Runs the Extract stage: parses the supplied Tally XML export and populates the job's staging records.</summary>
    [HttpPost("{id:guid}/extract-tally")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExtractTally(Guid id, [FromBody] ExtractTallyDataRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ExtractTallyDataCommand(request.CompanyId, id, request.TallyXmlContent), cancellationToken);
        return Ok(result);
    }

    /// <summary>Runs the Validate stage over this job's extracted staging records, flagging mapping errors before Load.</summary>
    [HttpPost("{id:guid}/validate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Validate(Guid id, [FromBody] MigrationJobActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ValidateMigrationStagingRecordsCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Runs the Load stage: commits validated staging records into live FusionOS entities.</summary>
    [HttpPost("{id:guid}/load")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Load(Guid id, [FromBody] MigrationJobActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new LoadMigrationStagingRecordsCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }

    /// <summary>Runs the final Reconcile stage: compares loaded record counts/totals against the source extract and marks the job Completed (or flags discrepancies).</summary>
    [HttpPost("{id:guid}/reconcile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reconcile(Guid id, [FromBody] MigrationJobActionRequest request, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ReconcileMigrationJobCommand(request.CompanyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record CreateMigrationJobRequest(Guid CompanyId, string SourceSystem);
public sealed record ExtractTallyDataRequest(Guid CompanyId, string TallyXmlContent);
public sealed record MigrationJobActionRequest(Guid CompanyId);
