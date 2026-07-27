using FusionOS.SharedKernel;

namespace FusionOS.Modules.Ai.Domain.Copilot;

/// <summary>
/// Phase 8 — AI Copilot. `docs/blueprint/12_AI_PLATFORM.md` §4 describes the Copilot as
/// "the conversational surface... a client of the same /api/v1/ai/... and module APIs
/// any other client uses, plus a retrieval layer... over... permitted business data
/// scoped to the asking user's RBAC permissions". This aggregate is the durable
/// conversation record that surface persists to: a session (one open conversation) owning
/// an ordered list of <see cref="CopilotMessage"/> turns.
///
/// <see cref="UserId"/> is an opaque cross-module reference into Core's User — never
/// existence-validated here, same convention as Maintenance's
/// MaintenanceRequest.AssignedTechnicianUserId.
///
/// Deliberately does NOT contain any real language-model call — see
/// <c>IChatResponseProvider</c>'s doc comment (Application/Copilot/Contracts) for why:
/// this codebase has no LLM API key, no pgvector/Qdrant deployment, and no embedding
/// capability available, so SendCopilotMessageCommandHandler's response generation is a
/// deterministic template over retrieved data, not a real conversational model — same
/// "honest gap, not a faked one" restraint Recommendation's doc comment documents for
/// forecasting/OCR.
/// </summary>
public sealed class CopilotSession : TenantAggregateRoot
{
    private readonly List<CopilotMessage> _messages = new();

    public Guid UserId { get; private set; }
    public string Title { get; private set; } = default!;
    public DateTimeOffset StartedAt { get; private set; }
    public DateTimeOffset? LastMessageAt { get; private set; }
    public IReadOnlyList<CopilotMessage> Messages => _messages.AsReadOnly();

    private CopilotSession() { }

    public static CopilotSession Start(Guid companyId, Guid userId, string? title)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User id is required.", nameof(userId));

        var resolvedTitle = string.IsNullOrWhiteSpace(title) ? "New conversation" : title.Trim();
        if (resolvedTitle.Length > 200)
            resolvedTitle = resolvedTitle[..200];

        var startedAt = DateTimeOffset.UtcNow;

        return new CopilotSession
        {
            CompanyId = companyId,
            UserId = userId,
            Title = resolvedTitle,
            StartedAt = startedAt,
            LastMessageAt = startedAt,
        };
    }

    /// <summary>
    /// Appends a turn to the conversation and bumps <see cref="LastMessageAt"/>. Called
    /// once for the user's own message and once more (same command, same unit of work)
    /// for the Copilot's generated reply — see SendCopilotMessageCommandHandler.
    /// </summary>
    public void AddMessage(CopilotRole role, string content)
    {
        _messages.Add(CopilotMessage.Create(role, content));
        LastMessageAt = DateTimeOffset.UtcNow;
    }
}
