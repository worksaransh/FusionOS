namespace FusionOS.Modules.Ai.Application.Search.Contracts;

/// <summary>
/// The result of a natural-language search (12_AI_PLATFORM.md §4 "Natural-language
/// search"). <see cref="RecognizedIntent"/> names which internal intent the query
/// text was matched to (or "Unrecognized"). <see cref="Explanation"/> is a
/// plain-English one-liner describing what was actually searched for and why,
/// so the user can see the translation was honest rather than magic.
/// <see cref="Results"/> is deliberately <c>object?</c> — it is whatever shape
/// the underlying, already-RBAC-gated query returned (a <c>PagedResult&lt;T&gt;</c>,
/// a filtered list, or null for the fallback/unrecognized case) passed through
/// as-is; this handler never reshapes or re-queries the underlying data itself.
/// </summary>
public sealed record AiSearchResultDto(string RecognizedIntent, string Explanation, object? Results);
