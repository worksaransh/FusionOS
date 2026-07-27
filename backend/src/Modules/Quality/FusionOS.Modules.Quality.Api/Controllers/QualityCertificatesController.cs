using FusionOS.Modules.Quality.Application.Certificates.Commands.GenerateCertificateOfAnalysis;
using FusionOS.Modules.Quality.Application.Certificates.Queries.GetCertificateByInspectionId;
using FusionOS.Modules.Quality.Application.Certificates.Queries.ListCertificates;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Quality.Api.Controllers;

/// <summary>
/// Phase 5 — Quality: Certificates of Analysis/Conformance (COA/COC). Generated once for a
/// Passed Inspection (idempotent per inspection — see GenerateCertificateOfAnalysisCommand's
/// doc comment); no PDF is produced, the response is structured data a caller/frontend
/// renders/prints (see CertificatesPanel.tsx).
/// </summary>
[ApiController]
[Route("api/v1/quality/certificates")]
public sealed class QualityCertificatesController : ControllerBase
{
    private readonly ISender _sender;

    public QualityCertificatesController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Generate([FromBody] GenerateCertificateOfAnalysisRequest request, CancellationToken cancellationToken)
    {
        var command = new GenerateCertificateOfAnalysisCommand(request.CompanyId, request.InspectionId, request.IssuedByUserId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByInspectionId), new { inspectionId = result.InspectionId, companyId = request.CompanyId }, result);
    }

    [HttpGet("by-inspection/{inspectionId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByInspectionId(Guid inspectionId, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCertificateByInspectionIdQuery(companyId, inspectionId), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListCertificatesQuery(companyId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record GenerateCertificateOfAnalysisRequest(Guid CompanyId, Guid InspectionId, Guid IssuedByUserId);
