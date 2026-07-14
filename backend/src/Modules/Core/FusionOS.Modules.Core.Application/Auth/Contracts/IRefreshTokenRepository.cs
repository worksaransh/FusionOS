using FusionOS.Modules.Core.Domain.Identity;

namespace FusionOS.Modules.Core.Application.Auth.Contracts;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken token, CancellationToken ct);
    Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct);
}
