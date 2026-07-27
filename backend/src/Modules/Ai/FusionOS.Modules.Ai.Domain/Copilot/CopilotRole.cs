namespace FusionOS.Modules.Ai.Domain.Copilot;

/// <summary>Who authored a <see cref="CopilotMessage"/> — the user asking, or the Copilot's generated reply.</summary>
public enum CopilotRole
{
    User,
    Assistant,
}
