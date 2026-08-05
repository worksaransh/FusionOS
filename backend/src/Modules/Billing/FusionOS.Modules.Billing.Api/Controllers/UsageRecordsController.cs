using FusionOS.Modules.Billing.Application.UsageRecords.Commands.RecordUsage;
using FusionOS.Modules.Billing.Application.UsageRecords.Queries.ListUsageRecordsBySubscription;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FusionOS.Modules.Billing.Api.Controllers;

/// <summary>Append-only usage metering against a Subscription — see UsageRecord's class doc comment for why there's no update/deactivate route.</summary>
[ApiController]
[Route("api/v1/billing/usage-records")]
public sealed class UsageRecordsController : ControllerBase
{
    private readonly ISender _sender;

    public UsageRecordsController(ISender sender) => _sender = sender;

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Record([FromBody] RecordUsageRequest request, CancellationToken cancellationToken)
    {
        var command = new RecordUsageCommand(request.CompanyId, request.SubscriptionId, request.MetricType, request.Quantity);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(ListBySubscription), new { companyId = request.CompanyId, subscriptionId = request.SubscriptionId }, result);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListBySubscription(
        [FromQuery] Guid companyId, [FromQuery] Guid subscriptionId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _sender.Send(new ListUsageRecordsBySubscriptionQuery(companyId, subscriptionId, page, pageSize), cancellationToken);
        return Ok(result);
    }
}

public sealed record RecordUsageRequest(Guid CompanyId, Guid SubscriptionId, string MetricType, int Quantity);
