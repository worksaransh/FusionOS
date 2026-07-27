namespace FusionOS.Modules.Ai.Application.Copilot.Retrieval;

/// <summary>
/// What <see cref="CopilotRetrievalStep"/> resolves to when the underlying AI Search
/// query recognized an intent but the asking user lacks the permission it (or its
/// resolved intent) requires — <see cref="ForbiddenException"/> caught and turned into
/// data instead of letting it fail the whole SendCopilotMessage command with an
/// unrelated-looking 403 mid-conversation. <see cref="Detail"/> is the exception's own
/// message (e.g. "The current user does not have the 'crm.lead.read' permission.").
/// </summary>
public sealed record CopilotAccessDenied(string Detail);
