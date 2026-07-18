using System.Text;
using FusionOS.BuildingBlocks.Application.Csv;
using FusionOS.Modules.Procurement.Application.Suppliers.Commands.CreateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Commands.DeactivateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Commands.UpdateSupplier;
using FusionOS.Modules.Procurement.Application.Suppliers.Queries.GetSupplierById;
using FusionOS.Modules.Procurement.Application.Suppliers.Queries.ListSuppliers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Procurement.Api.Controllers;

[ApiController]
[Route("api/v1/procurement/suppliers")]
public sealed class SuppliersController : ControllerBase
{
    private readonly ISender _sender;

    public SuppliersController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateSupplierCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id, companyId = command.CompanyId }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetSupplierByIdQuery(companyId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] Guid companyId, [FromQuery] string? search = null, [FromQuery] string? format = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListSuppliersQuery(companyId, search, page, pageSize), cancellationToken);
        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = CsvWriter.Write(result.Data);
            return File(Encoding.UTF8.GetBytes(csv), "text/csv", "suppliers.csv");
        }
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupplierRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateSupplierCommand(request.CompanyId, id, request.Name, request.ContactEmail, request.ContactPhone);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:guid}/deactivate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Deactivate(Guid id, [FromQuery] Guid companyId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeactivateSupplierCommand(companyId, id), cancellationToken);
        return Ok(result);
    }
}

public sealed record UpdateSupplierRequest(Guid CompanyId, string Name, string? ContactEmail, string? ContactPhone);
