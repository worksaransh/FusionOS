using FluentValidation;

namespace FusionOS.Modules.Ai.Application.Copilot.Commands.SendCopilotMessage;

public sealed class SendCopilotMessageValidator : AbstractValidator<SendCopilotMessageCommand>
{
    public SendCopilotMessageValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Message).NotEmpty().MaximumLength(4000);
    }
}
