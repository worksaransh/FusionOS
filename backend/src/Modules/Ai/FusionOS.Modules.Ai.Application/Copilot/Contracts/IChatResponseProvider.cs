namespace FusionOS.Modules.Ai.Application.Copilot.Contracts;

/// <summary>
/// Generates the Copilot's reply text for a single turn, given the user's raw message and
/// whatever the retrieval step (<see cref="Retrieval.CopilotRetrievalStep"/>) found for it.
///
/// THIS IS THE INTENDED PRODUCTION EXTENSION SEAM. `docs/blueprint/12_AI_PLATFORM.md` §4
/// describes the Copilot as a client of a real retrieval layer feeding a real conversational
/// model — that requires an LLM API key (Anthropic/OpenAI/etc.), neither of which is
/// available in this environment. The production path once one is provisioned is: register
/// a new implementation of this interface (e.g. `AnthropicChatResponseProvider`, most
/// naturally living in Infrastructure since it would call out to an external HTTP API) in
/// place of `TemplateChatResponseProvider` in AiModule.cs's DI registration, that calls the
/// LLM with the retrieved context (see the method signature below) passed as grounding
/// context alongside the user's message, so the model answers from real FusionOS data
/// instead of inventing an answer. Nothing else in SendCopilotMessageCommandHandler needs
/// to change for that swap: it only ever depends on this interface.
///
/// `TemplateChatResponseProvider` (Copilot/Chat, same project — it has no external
/// dependency of its own, unlike a real LLM call, so there is nothing Infrastructure-specific
/// about it yet) is the deliberate, honest placeholder shipped today instead: a fully
/// deterministic implementation with no external call, that interpolates real retrieved
/// numbers into a sentence when an intent was recognized, and plainly says so when one
/// wasn't — never a canned "as an AI..." response pretending to understand something it didn't.
/// </summary>
public interface IChatResponseProvider
{
    /// <param name="userMessage">The raw text the user sent this turn.</param>
    /// <param name="retrievedContext">
    /// Whatever the retrieval step resolved for this message — today, an
    /// <c>AiSearchResultDto</c> (reused from AI Search), a <see cref="Retrieval.CopilotAccessDenied"/>
    /// if the user lacked the permission the resolved intent required, or null if no intent
    /// was recognized. Typed as <c>object?</c> so a future LLM-backed provider can accept a
    /// richer/different retrieval shape without changing this interface.
    /// </param>
    Task<string> GenerateResponseAsync(string userMessage, object? retrievedContext, CancellationToken cancellationToken);
}
