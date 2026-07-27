namespace FusionOS.Modules.Ai.Domain.Copilot;

/// <summary>
/// One turn in a <see cref="CopilotSession"/> — either the user's message or the Copilot's
/// generated reply. Owned entirely by its parent session: no own audit/tenant columns,
/// same "owned by parent" convention as Quality's InspectionItem or Manufacturing's
/// WorkOrderComponent — its lifecycle is entirely the session's to manage.
/// </summary>
public sealed class CopilotMessage
{
    public Guid Id { get; private set; }
    public CopilotRole Role { get; private set; }
    public string Content { get; private set; } = default!;
    public DateTimeOffset SentAt { get; private set; }

    private CopilotMessage() { }

    internal static CopilotMessage Create(CopilotRole role, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content is required.", nameof(content));

        return new CopilotMessage
        {
            Id = Guid.NewGuid(),
            Role = role,
            Content = content.Trim(),
            SentAt = DateTimeOffset.UtcNow,
        };
    }
}
