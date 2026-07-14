using FluentValidation;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Refresh;

public sealed class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
