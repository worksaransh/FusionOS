using FusionOS.BuildingBlocks.Application.Abstractions;
using FusionOS.Modules.Ai.Application.Search.Contracts;

namespace FusionOS.Modules.Ai.Application.Search.Queries.RunAiSearch;

/// <summary>
/// Natural-language search entry point (12_AI_PLATFORM.md §4). <see cref="QueryText"/>
/// is parsed by <see cref="AiSearchIntentResolver"/> into one of a small set of
/// known intents, each of which dispatches to an EXISTING read query belonging
/// to another module via <c>ISender</c> — never a direct DbContext query from
/// here — so that query's own <c>IRequirePermission</c>/<c>AuthorizationBehavior</c>
/// gate still applies. This query itself is also gated on "ai.search.query" so a
/// caller needs that grant just to use the search surface at all, on top of
/// whatever permission the resolved underlying query additionally requires.
/// </summary>
public sealed record RunAiSearchQuery(Guid CompanyId, string QueryText) : IQuery<AiSearchResultDto>, IRequirePermission
{
    public string[] RequiredPermissions => new[] { "ai.search.query" };
}
