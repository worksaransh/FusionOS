namespace FusionOS.Modules.Ai.Application.Search;

/// <summary>
/// The small, fixed set of intents <see cref="AiSearchIntentResolver"/> currently
/// recognizes. This is a deliberately narrow starting set (12_AI_PLATFORM.md §4:
/// "translated into structured queries against each module's published read
/// APIs") — not a claim of general NLP coverage.
/// </summary>
public enum AiSearchIntent
{
    /// <summary>No keyword pattern below matched — see <see cref="AiSearchIntentResolver.ExamplePhrases"/> for what to suggest the user try instead.</summary>
    Unrecognized,
    OutstandingInvoices,
    OutOfStockProducts,
    LowStockProducts,
    OpenLeads,
    PendingMaintenanceRequests,
}

/// <summary>
/// Resolves free-text search queries into one of a small set of known intents
/// using honest, deterministic, case-insensitive substring matching — NOT a
/// real NLP/ML model. This is intentional and documented here rather than
/// hidden: 12_AI_PLATFORM.md §4 describes natural-language search as "a
/// translation layer over existing authorized queries, not a bypass of
/// RBAC/permission checks" — it says nothing about *how* the translation is
/// implemented, and standing up a real embedding-based classifier
/// (02_TECH_STACK.md's pgvector mention) requires a vector store and an
/// embedding API this environment cannot provision. Swapping this resolver's
/// body for a real classifier later is the natural evolution once that
/// infrastructure exists; every call site only depends on the
/// <see cref="AiSearchIntent"/> enum, so that swap is contained here.
///
/// Keyword lists per intent (all matches case-insensitive, checked in the
/// order below — more specific phrases are checked before the generic ones
/// they'd otherwise be swallowed by):
/// <list type="bullet">
/// <item>OutOfStockProducts: "out of stock", "out-of-stock"</item>
/// <item>LowStockProducts: "low stock", "low-stock", "running low"</item>
/// <item>OutstandingInvoices: "overdue invoice", "unpaid invoice", "outstanding invoice", "invoice"</item>
/// <item>OpenLeads: "open lead", "my lead", "lead"</item>
/// <item>PendingMaintenanceRequests: "pending maintenance", "maintenance request", "maintenance"</item>
/// </list>
/// </summary>
public static class AiSearchIntentResolver
{
    public static readonly IReadOnlyList<string> ExamplePhrases = new[]
    {
        "overdue invoices",
        "unpaid invoices",
        "low stock products",
        "out of stock products",
        "open leads",
        "my leads",
        "pending maintenance requests",
    };

    public static AiSearchIntent Resolve(string? queryText)
    {
        if (string.IsNullOrWhiteSpace(queryText))
            return AiSearchIntent.Unrecognized;

        var text = queryText.Trim().ToLowerInvariant();

        if (Contains(text, "out of stock", "out-of-stock"))
            return AiSearchIntent.OutOfStockProducts;

        if (Contains(text, "low stock", "low-stock", "running low"))
            return AiSearchIntent.LowStockProducts;

        if (Contains(text, "overdue invoice", "unpaid invoice", "outstanding invoice", "invoice"))
            return AiSearchIntent.OutstandingInvoices;

        if (Contains(text, "open lead", "my lead", "lead"))
            return AiSearchIntent.OpenLeads;

        if (Contains(text, "pending maintenance", "maintenance request", "maintenance"))
            return AiSearchIntent.PendingMaintenanceRequests;

        return AiSearchIntent.Unrecognized;
    }

    private static bool Contains(string haystack, params string[] needles) =>
        needles.Any(haystack.Contains);
}
