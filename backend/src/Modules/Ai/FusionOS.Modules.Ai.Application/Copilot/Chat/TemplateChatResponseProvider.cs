using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Copilot.Contracts;
using FusionOS.Modules.Ai.Application.Copilot.Retrieval;
using FusionOS.Modules.Ai.Application.Search;
using FusionOS.Modules.Ai.Application.Search.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Inventory.Application.Reports.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;

namespace FusionOS.Modules.Ai.Application.Copilot.Chat;

/// <summary>
/// The deliberate, honest placeholder <see cref="IChatResponseProvider"/> implementation
/// shipped today — see that interface's doc comment for the full rationale and the
/// production swap-in path. No external call, no randomness: given the same message and
/// retrieval result twice, this returns the same text twice. When the retrieval step
/// (<see cref="CopilotRetrievalStep"/>, wrapping AI Search's <c>RunAiSearchQuery</c>)
/// recognized an intent, the reply interpolates the REAL numbers/records it found. When it
/// didn't, the reply says so plainly rather than pretending to understand — matching
/// 12_AI_PLATFORM.md §4's framing of the Copilot as a translation layer over real data,
/// never a black box guessing at an answer.
/// </summary>
public sealed class TemplateChatResponseProvider : IChatResponseProvider
{
    public Task<string> GenerateResponseAsync(string userMessage, object? retrievedContext, CancellationToken cancellationToken)
    {
        var response = retrievedContext switch
        {
            CopilotAccessDenied denied =>
                $"I can answer that, but your account doesn't have the permission it requires ({denied.Detail}). Ask an administrator to grant it if you need this.",

            AiSearchResultDto result => FormatSearchResult(result),

            _ => Unrecognized(),
        };

        return Task.FromResult(response);
    }

    private static string FormatSearchResult(AiSearchResultDto result)
    {
        if (result.RecognizedIntent == nameof(AiSearchIntent.Unrecognized) || result.Results is null)
            return Unrecognized();

        return result switch
        {
            { RecognizedIntent: nameof(AiSearchIntent.OutstandingInvoices), Results: PagedResult<InvoiceDto> invoices } => FormatInvoices(invoices),
            { RecognizedIntent: nameof(AiSearchIntent.OpenLeads), Results: PagedResult<LeadDto> leads } => FormatLeads(leads),
            { RecognizedIntent: nameof(AiSearchIntent.OutOfStockProducts), Results: IReadOnlyList<StockValuationLineDto> outOfStock } => FormatStock(outOfStock, "out-of-stock", isOutOfStock: true),
            { RecognizedIntent: nameof(AiSearchIntent.LowStockProducts), Results: IReadOnlyList<StockValuationLineDto> lowStock } => FormatStock(lowStock, "low-stock", isOutOfStock: false),
            { RecognizedIntent: nameof(AiSearchIntent.PendingMaintenanceRequests), Results: PagedResult<MaintenanceRequestDto> requests } => FormatMaintenance(requests),
            _ => $"I recognized that as \"{result.RecognizedIntent}\", but couldn't format the retrieved result into a reply — try rephrasing.",
        };
    }

    private const string InvoiceCaveat = "(Sales tracks invoice status as Draft/Issued only today, so \"outstanding\" is approximated as Issued invoices until Finance adds AR aging data.)";

    private static string FormatInvoices(PagedResult<InvoiceDto> invoices)
    {
        if (invoices.Data.Count == 0)
            return $"Good news — I didn't find any outstanding invoices. {InvoiceCaveat}";

        var largest = invoices.Data.OrderByDescending(i => i.TotalAmount).First();
        return $"I found {invoices.Data.Count} outstanding invoice(s) — the largest is ₹{largest.TotalAmount:N2} (invoice {largest.Id}, customer {largest.CustomerId}). Would you like the full list? {InvoiceCaveat}";
    }

    private static string FormatLeads(PagedResult<LeadDto> leads)
    {
        if (leads.Data.Count == 0)
            return "I didn't find any open leads right now.";

        var sample = leads.Data.First();
        return $"I found {leads.Data.Count} open lead(s) — for example \"{sample.Name}\" (status {sample.Status}). Would you like the full list?";
    }

    private static string FormatStock(IReadOnlyList<StockValuationLineDto> lines, string label, bool isOutOfStock)
    {
        if (lines.Count == 0)
            return isOutOfStock
                ? "Good news — no products are out of stock."
                : "Good news — no products are running low on stock.";

        var worst = lines.OrderBy(l => l.OnHandQuantity).First();
        return $"I found {lines.Count} {label} product(s) — the lowest is \"{worst.Name}\" ({worst.Sku}) with {worst.OnHandQuantity:0.##} unit(s) on hand. Would you like the full list?";
    }

    private static string FormatMaintenance(PagedResult<MaintenanceRequestDto> requests)
    {
        if (requests.Data.Count == 0)
            return "Good news — there are no pending maintenance requests.";

        var oldest = requests.Data.OrderBy(r => r.ReportedAt).First();
        return $"I found {requests.Data.Count} pending maintenance request(s) — the oldest is a {oldest.Type} request reported on {oldest.ReportedAt:yyyy-MM-dd} (status {oldest.Status}). Would you like the full list?";
    }

    private static string Unrecognized() =>
        "I couldn't map that to a specific FusionOS query yet — try asking about overdue invoices, low stock, out of stock, open leads, or pending maintenance requests.";
}
