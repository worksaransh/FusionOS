namespace FusionOS.Modules.Sales.Application.EWayBills.Contracts;

/// <summary>
/// Abstraction over the GST e-way bill generation API — a related but distinct GSTN system
/// from the Invoice Registration Portal (IGstIrpClient generates an invoice's IRN/QR; this
/// generates the transport document covering a physical dispatch). Same "no real GSP
/// credential available today" reasoning as IGstIrpClient — see
/// Infrastructure/Gst/SandboxEWayBillClient for the default fake implementation.
/// </summary>
public interface IEWayBillClient
{
    Task<EWayBillGenerationResult> GenerateAsync(EWayBillGenerationPayload payload, CancellationToken cancellationToken = default);
}

public sealed record EWayBillGenerationPayload(Guid CompanyId, Guid DispatchId, DateTimeOffset DispatchDate);

public sealed record EWayBillGenerationResult(string EwbNumber, DateTimeOffset GeneratedAt, DateTimeOffset ValidUntil);
