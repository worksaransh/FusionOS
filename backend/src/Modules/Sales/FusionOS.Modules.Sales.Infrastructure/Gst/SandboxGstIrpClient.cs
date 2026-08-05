using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;

namespace FusionOS.Modules.Sales.Infrastructure.Gst;

/// <summary>
/// Default IGstIrpClient implementation — deliberately NOT a real GST Invoice Registration
/// Portal integration. Real submission requires a GSP (GST Suvidha Provider) account, an
/// external commercial credential this codebase has no way to obtain on its own. Rather than
/// leave GenerateEInvoiceCommandHandler entirely unimplementable and untestable until that
/// account exists, this sandbox implementation generates a deterministic, correctly-shaped
/// fake response — a 64-hex-character IRN (matching the IRP's real hash-based format) and a
/// compact JSON QR payload — so the feature's validation/idempotency/GL-adjacent logic can be
/// built and tested end-to-end today.
///
/// This must be swapped for a real HTTP-backed IGstIrpClient before this feature is used
/// against real invoices — nothing this class returns is ever submitted to the actual GSTN
/// systems, and the "IRN" it fabricates would be rejected by any real downstream GST filing.
/// </summary>
public sealed class SandboxGstIrpClient : IGstIrpClient
{
    public Task<GstIrpSubmissionResult> SubmitInvoiceAsync(GstIrpInvoicePayload payload, CancellationToken cancellationToken = default)
    {
        var irn = ComputeSandboxIrn(payload);
        var qrCodePayload = JsonSerializer.Serialize(new
        {
            Irn = irn,
            payload.CompanyId,
            payload.InvoiceId,
            payload.TotalAmount,
            InvoiceDate = payload.InvoiceDate.ToString("O"),
            Sandbox = true,
        });
        var ackNumber = $"SANDBOX-{payload.InvoiceId:N}";

        return Task.FromResult(new GstIrpSubmissionResult(irn, qrCodePayload, ackNumber, DateTimeOffset.UtcNow));
    }

    private static string ComputeSandboxIrn(GstIrpInvoicePayload payload)
    {
        var input = $"{payload.CompanyId}|{payload.InvoiceId}|{payload.InvoiceDate:O}|{payload.TotalAmount}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
