using FusionOS.Modules.Sales.Application.Dispatches.Queries.ListMyDispatches;
using FusionOS.Modules.Sales.Application.Invoices.Queries.ListMyInvoices;
using FusionOS.Modules.Sales.Application.Quotations.Commands.AcceptOwnQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Commands.RejectOwnQuotation;
using FusionOS.Modules.Sales.Application.Quotations.Queries.ListMyQuotations;
using FusionOS.Modules.Sales.Application.SalesOrders.Queries.ListMySalesOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Sales.Api.Controllers;

/// <summary>
/// The customer's own read-mostly self-service surface (Customer Portal,
/// 2026-07-22). Every action here requires the "CustomerPortal" authorization
/// policy (Program.cs) - a valid, non-expired JWT carrying "token_type":
/// "customer" - and NEVER trusts a customer-supplied CustomerId: the
/// CustomerId used for every query/command below comes only from the
/// "customer_id" claim on the caller's own validated token (CustomerId()
/// below), the same way HttpCurrentUserContext derives sub/company_id for
/// staff callers. This is the entire authorization story for this controller
/// - there is no IRequirePermission/RBAC involved (see
/// PortalLoginCommandHandler's doc comment for why a customer token could
/// never satisfy one anyway).
/// </summary>
[ApiController]
[Route("api/v1/sales/portal")]
[Authorize(Policy = "CustomerPortal")]
public sealed class PortalController : ControllerBase
{
    private readonly ISender _sender;

    public PortalController(ISender sender) => _sender = sender;

    private Guid CustomerId
    {
        get
        {
            var claim = User.FindFirst("customer_id")?.Value;
            return Guid.TryParse(claim, out var id)
                ? id
                : throw new InvalidOperationException("Customer portal token is missing its customer_id claim.");
        }
    }

    [HttpGet("quotations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListQuotations([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMyQuotationsQuery(CustomerId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpPost("quotations/{id:guid}/accept")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AcceptQuotation(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new AcceptOwnQuotationCommand(CustomerId, id), cancellationToken);
        return Ok(result);
    }

    [HttpPost("quotations/{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectQuotation(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new RejectOwnQuotationCommand(CustomerId, id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("sales-orders")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListSalesOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMySalesOrdersQuery(CustomerId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("invoices")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListInvoices([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMyInvoicesQuery(CustomerId, page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("dispatches")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDispatches([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListMyDispatchesQuery(CustomerId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}
