using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Identity;

/// <summary>
/// A rotating refresh token (07_SECURITY.md). We never store the raw token —
/// only a SHA-256 hash of it — so a leaked database dump cannot be replayed
/// directly. CompanyId/BranchId are captured at issuance so a refresh does not
/// need to re-run the full login/role-lookup flow to reissue the same context.
/// </summary>
public sealed class RefreshToken : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid? CompanyId { get; private set; }
    public Guid? BranchId { get; private set; }
    public string TokenHash { get; private set; } = default!;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }

    public bool IsActive => RevokedAt is null && DateTimeOffset.UtcNow < ExpiresAt;

    private RefreshToken() { }

    public static RefreshToken Issue(Guid userId, Guid? companyId, Guid? branchId, string tokenHash, DateTimeOffset expiresAt) => new()
    {
        UserId = userId,
        CompanyId = companyId,
        BranchId = branchId,
        TokenHash = tokenHash,
        ExpiresAt = expiresAt,
    };

    public void Revoke(Guid? replacedByTokenId = null)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        ReplacedByTokenId = replacedByTokenId;
    }
}
