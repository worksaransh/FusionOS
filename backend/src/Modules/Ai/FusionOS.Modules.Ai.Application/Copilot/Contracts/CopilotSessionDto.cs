namespace FusionOS.Modules.Ai.Application.Copilot.Contracts;

public sealed record CopilotMessageDto(Guid Id, string Role, string Content, DateTimeOffset SentAt);

public sealed record CopilotSessionDto(
    Guid Id,
    Guid UserId,
    string Title,
    DateTimeOffset StartedAt,
    DateTimeOffset? LastMessageAt,
    IReadOnlyList<CopilotMessageDto> Messages);

/// <summary>Single place that turns a CopilotSession aggregate into its DTO, shared by every handler that returns one.</summary>
public static class CopilotSessionMapper
{
    public static CopilotSessionDto ToDto(Domain.Copilot.CopilotSession session) => new(
        session.Id,
        session.UserId,
        session.Title,
        session.StartedAt,
        session.LastMessageAt,
        session.Messages.Select(m => new CopilotMessageDto(m.Id, m.Role.ToString(), m.Content, m.SentAt)).ToList());
}
