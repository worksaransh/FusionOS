using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Search.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Contracts;
using FusionOS.Modules.Crm.Application.Leads.Queries.ListLeads;
using FusionOS.Modules.Inventory.Application.Reports.Contracts;
using FusionOS.Modules.Inventory.Application.Reports.Queries.GetStockValuationReport;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Contracts;
using FusionOS.Modules.Maintenance.Application.MaintenanceRequests.Queries.ListMaintenanceRequests;
using FusionOS.Modules.Sales.Application.Invoices.Contracts;
using FusionOS.Modules.Sales.Application.Invoices.Queries.ListInvoices;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Search.Queries.RunAiSearch;

/// <summary>
/// Translates <see cref="RunAiSearchQuery.QueryText"/> into an existing,
/// RBAC-gated read query in another module and passes its result straight
/// through — see <see cref="AiSearchIntentResolver"/> for the exact keyword
/// list per intent, and <see cref="RunAiSearchQuery"/> for why dispatching via
/// <see cref="ISender"/> (rather than querying a DbContext directly) is what
/// keeps this a translation layer instead of an RBAC bypass.
///
/// A naive fixed page size (100) is used for every dispatched list query
/// below rather than true pagination — acceptable for a first search slice
/// operating over the same modest per-company record counts every other
/// "List..." UI panel in this codebase already assumes; a production version
/// would page through results or push filtering server-side into each
/// module's query instead of filtering the page client-side here.
/// </summary>
public sealed class RunAiSearchQueryHandler : IRequestHandler<RunAiSearchQuery, AiSearchResultDto>
{
    private const int MaxResultsPerIntent = 100;

    private readonly ISender _sender;

    public RunAiSearchQueryHandler(ISender sender) => _sender = sender;

    public async Task<AiSearchResultDto> Handle(RunAiSearchQuery request, CancellationToken cancellationToken)
    {
        var intent = AiSearchIntentResolver.Resolve(request.QueryText);

        return intent switch
        {
            AiSearchIntent.OutstandingInvoices => await HandleOutstandingInvoices(request.CompanyId, cancellationToken),
            AiSearchIntent.OutOfStockProducts => await HandleStockLevel(request.CompanyId, outOfStockOnly: true, cancellationToken),
            AiSearchIntent.LowStockProducts => await HandleStockLevel(request.CompanyId, outOfStockOnly: false, cancellationToken),
            AiSearchIntent.OpenLeads => await HandleOpenLeads(request.CompanyId, cancellationToken),
            AiSearchIntent.PendingMaintenanceRequests => await HandlePendingMaintenanceRequests(request.CompanyId, cancellationToken),
            _ => Unrecognized(),
        };
    }

    private async Task<AiSearchResultDto> HandleOutstandingInvoices(Guid companyId, CancellationToken cancellationToken)
    {
        var page = await _sender.Send(new ListInvoicesQuery(companyId, 1, MaxResultsPerIntent), cancellationToken);

        // Sales' Invoice has no DueDate or payment-status field yet (Draft/Issued
        // only, see InvoiceStatus) — "overdue"/"unpaid" is honestly approximated
        // as "Issued" (sent to the customer, not merely a draft) until Finance
        // adds real AR aging data.
        var filtered = page.Data.Where(i => i.Status == "Issued").ToList();

        return new AiSearchResultDto(
            nameof(AiSearchIntent.OutstandingInvoices),
            "Interpreted as: outstanding invoices. Sales currently tracks invoice status as Draft/Issued only (no due-date or payment field yet), so this shows Issued invoices as the closest available proxy for \"overdue\"/\"unpaid\" until Finance adds accounts-receivable aging data.",
            new PagedResult<InvoiceDto>(filtered, 1, MaxResultsPerIntent, filtered.Count));
    }

    private async Task<AiSearchResultDto> HandleStockLevel(Guid companyId, bool outOfStockOnly, CancellationToken cancellationToken)
    {
        var report = await _sender.Send(new GetStockValuationReportQuery(companyId), cancellationToken);

        // Inventory's Product schema has no ReorderPoint/reorder-threshold field
        // yet, so "low stock" cannot be computed against a real per-product
        // threshold. Documented, fixed approximations: "out of stock" = zero
        // on hand (objective); "low stock" = greater than zero but at or below
        // a naive fixed threshold of 10 units — a placeholder until Inventory
        // gains a real reorder-point concept per product.
        const decimal lowStockThreshold = 10m;

        var filtered = outOfStockOnly
            ? report.Lines.Where(l => l.OnHandQuantity <= 0m).ToList()
            : report.Lines.Where(l => l.OnHandQuantity > 0m && l.OnHandQuantity <= lowStockThreshold).ToList();

        var explanation = outOfStockOnly
            ? "Interpreted as: out-of-stock products — products with zero units on hand across all warehouses."
            : $"Interpreted as: low-stock products — products with more than zero but at most {lowStockThreshold:0} units on hand. Inventory does not yet track a per-product reorder point, so this uses a fixed placeholder threshold rather than a real one.";

        return new AiSearchResultDto(
            outOfStockOnly ? nameof(AiSearchIntent.OutOfStockProducts) : nameof(AiSearchIntent.LowStockProducts),
            explanation,
            filtered);
    }

    private async Task<AiSearchResultDto> HandleOpenLeads(Guid companyId, CancellationToken cancellationToken)
    {
        var page = await _sender.Send(new ListLeadsQuery(companyId, null, 1, MaxResultsPerIntent), cancellationToken);

        // "Open" = not yet at a terminal LeadStatus (Converted or Disqualified).
        var filtered = page.Data.Where(l => l.Status != "Converted" && l.Status != "Disqualified").ToList();

        return new AiSearchResultDto(
            nameof(AiSearchIntent.OpenLeads),
            "Interpreted as: open leads — leads not yet Converted or Disqualified.",
            new PagedResult<LeadDto>(filtered, 1, MaxResultsPerIntent, filtered.Count));
    }

    private async Task<AiSearchResultDto> HandlePendingMaintenanceRequests(Guid companyId, CancellationToken cancellationToken)
    {
        var page = await _sender.Send(new ListMaintenanceRequestsQuery(companyId, null, 1, MaxResultsPerIntent), cancellationToken);

        // "Pending" = not yet Completed (i.e. Open or InProgress).
        var filtered = page.Data.Where(m => m.Status != "Completed").ToList();

        return new AiSearchResultDto(
            nameof(AiSearchIntent.PendingMaintenanceRequests),
            "Interpreted as: pending maintenance requests — requests that are Open or InProgress (not yet Completed).",
            new PagedResult<MaintenanceRequestDto>(filtered, 1, MaxResultsPerIntent, filtered.Count));
    }

    private static AiSearchResultDto Unrecognized() => new(
        nameof(AiSearchIntent.Unrecognized),
        "Could not match this query to a known search intent. Try one of: " + string.Join("; ", AiSearchIntentResolver.ExamplePhrases) + ".",
        null);
}
