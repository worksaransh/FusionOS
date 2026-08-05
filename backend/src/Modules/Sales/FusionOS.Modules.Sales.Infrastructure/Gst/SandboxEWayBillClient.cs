using FusionOS.Modules.Sales.Application.EWayBills.Contracts;

namespace FusionOS.Modules.Sales.Infrastructure.Gst;

/// <summary>
/// Default IEWayBillClient implementation — same sandbox reasoning as SandboxGstIrpClient:
/// real e-way bill generation requires a GSP account this codebase cannot obtain on its own.
/// Generates a deterministic, correctly-shaped fake e-way bill number (EWB + 12 digits,
/// matching the real system's format) with a validity window — real e-way bill validity is
/// distance-based (roughly one day per 200km, with a minimum of one day), which this sandbox
/// approximates with a flat, clearly-fake default rather than pretending to know a real
/// transport distance no field in this codebase captures yet.
/// </summary>
public sealed class SandboxEWayBillClient : IEWayBillClient
{
    private static readonly TimeSpan DefaultValidityWindow = TimeSpan.FromDays(1);

    public Task<EWayBillGenerationResult> GenerateAsync(EWayBillGenerationPayload payload, CancellationToken cancellationToken = default)
    {
        var generatedAt = DateTimeOffset.UtcNow;
        var ewbNumber = $"EWB{Math.Abs(payload.DispatchId.GetHashCode()):D12}";

        return Task.FromResult(new EWayBillGenerationResult(ewbNumber, generatedAt, generatedAt + DefaultValidityWindow));
    }
}
