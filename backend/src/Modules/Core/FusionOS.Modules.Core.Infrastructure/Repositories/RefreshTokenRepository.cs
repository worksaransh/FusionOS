using FusionOS.Modules.Core.Application.Auth.Contracts;
using FusionOS.Modules.Core.Domain.Identity;
using FusionOS.Modules.Core.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FusionOS.Modules.Core.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CoreDbContext _context;

    public RefreshTokenRepository(CoreDbContext context) => _context = context;

    public async Task AddAsync(RefreshToken token, CancellationToken ct) => await _context.RefreshTokens.AddAsync(token, ct);

    public Task<RefreshToken?> GetActiveByHashAsync(string tokenHash, CancellationToken ct) =>
        _context.RefreshTokens
            .Where(rt => rt.TokenHash == tokenHash && rt.RevokedAt == null && rt.ExpiresAt > DateTimeOffset.UtcNow)
            .FirstOrDefaultAsync(ct);
}
