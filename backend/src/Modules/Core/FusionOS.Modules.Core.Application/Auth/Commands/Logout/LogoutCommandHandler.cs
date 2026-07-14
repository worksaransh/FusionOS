using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Application.Companies.Contracts;
using MediatR;

namespace FusionOS.Modules.Core.Application.Auth.Commands.Logout;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IRefreshTokenRepository refreshTokens, IJwtTokenService tokenService, IUnitOfWork unitOfWork)
    {
        _refreshTokens = refreshTokens;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var hash = _tokenService.HashRefreshTokenValue(request.RefreshToken);
        var existing = await _refreshTokens.GetActiveByHashAsync(hash, cancellationToken);
        if (existing is not null)
        {
            existing.Revoke();
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
