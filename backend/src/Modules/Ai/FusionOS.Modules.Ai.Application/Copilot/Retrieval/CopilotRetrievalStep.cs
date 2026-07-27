using FusionOS.BuildingBlocks.Application.Exceptions;
using FusionOS.Modules.Ai.Application.Search.Contracts;
using FusionOS.Modules.Ai.Application.Search.Queries.RunAiSearch;
using MediatR;

namespace FusionOS.Modules.Ai.Application.Copilot.Retrieval;

/// <summary>
/// The Copilot's retrieval step (`docs/blueprint/12_AI_PLATFORM.md` §4: the Copilot is
/// "a retrieval layer... over... permitted business data scoped to the asking user's RBAC
/// permissions"). Genuinely reuses AI Search's intent resolution — <see cref="RunAiSearchQuery"/>
/// — which existed in this codebase by the time this class was written (the concurrent AI
/// Search workstream landed first): the same keyword-matched intents ("overdue invoices",
/// "open leads", "low stock", "out of stock", "pending maintenance") a user could ask AI
/// Search directly are what the Copilot answers from, dispatched through the same
/// <see cref="ISender"/> pipeline so RBAC (both "ai.search.query" and whichever module
/// permission the resolved intent needs) is enforced exactly as it would be for a direct
/// AI Search call. There is no separate/duplicate keyword resolver here — had
/// <c>RunAiSearchQuery</c> not existed yet, this class would instead have contained its own
/// minimal fallback matcher; it did exist, so this is a thin pass-through instead.
/// </summary>
public sealed class CopilotRetrievalStep
{
    private readonly ISender _sender;

    public CopilotRetrievalStep(ISender sender) => _sender = sender;

    /// <returns>
    /// An <see cref="AiSearchResultDto"/> (possibly with <c>RecognizedIntent</c> ==
    /// "Unrecognized" and null Results, if AI Search itself couldn't match anything), a
    /// <see cref="CopilotAccessDenied"/> if the user lacks the required permission, or
    /// null if <paramref name="userMessage"/> was blank.
    /// </returns>
    public async Task<object?> ResolveAsync(Guid companyId, string userMessage, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return null;

        try
        {
            return await _sender.Send(new RunAiSearchQuery(companyId, userMessage), cancellationToken);
        }
        catch (ForbiddenException ex)
        {
            return new CopilotAccessDenied(ex.Message);
        }
    }
}
