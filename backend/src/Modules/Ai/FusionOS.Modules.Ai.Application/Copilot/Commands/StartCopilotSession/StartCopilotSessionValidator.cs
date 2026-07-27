using FluentValidation;

namespace FusionOS.Modules.Ai.Application.Copilot.Commands.StartCopilotSession;

public sealed class StartCopilotSessionValidator : AbstractValidator<StartCopilotSessionCommand>
{
    public StartCopilotSessionValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Title).MaximumLength(200);
    }
}
