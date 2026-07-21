using System.Security.Cryptography;
using System.Text;
using FusionOS.SharedKernel;
using FusionOS.Modules.Core.Domain.FeatureFlags.Events;

namespace FusionOS.Modules.Core.Domain.FeatureFlags;

/// <summary>
/// A per-company feature flag — FusionOS has no feature-flag system anywhere else in the
/// codebase, so this is net-new (no FeatureManagement library, no flag table). Deliberately
/// per-company (TenantAggregateRoot), like everything else in this codebase, rather than a
/// global/system-wide entity: Company itself is the only concept in FusionOS that is
/// intentionally NOT tenant-scoped (it IS the tenant), and there is no existing precedent for
/// a "global, cross-company" record anywhere else (every catalog-like entity — e.g.
/// Marketplace's PluginListing — is still scoped per company). A genuinely global flag would
/// be new, unrequested scope; if that need arises later it would be its own entity, not a
/// nullable CompanyId bolted onto this one.
///
/// Key is the immutable, company-scoped business key (unique per company via a DB index on
/// (CompanyId, Key) — same convention as CostCenter.Code). Name/Description/RolloutPercentage
/// are mutable via UpdateDetails. IsEnabled flips via the dedicated Toggle() fast path — a
/// small, dedicated workflow-transition action kept separate from UpdateDetails, matching
/// this codebase's convention of narrow status-transition commands (NonConformanceReport.
/// UpdateStatus, PluginInstallation.Enable/Disable) rather than one generic update that does
/// everything.
///
/// RolloutPercentage (0-100, default 100) is an optional gradual-rollout knob, not a real
/// experimentation platform: no targeting rules, no sticky-assignment table, no
/// multi-variant tests — just a deterministic hash-mod-100 comparison of (Key + caller's
/// EvaluationId), see Evaluate().
/// </summary>
public sealed class FeatureFlag : TenantAggregateRoot
{
    public const int MinRolloutPercentage = 0;
    public const int MaxRolloutPercentage = 100;

    public string Key { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public int RolloutPercentage { get; private set; } = MaxRolloutPercentage;

    private FeatureFlag() { }

    public static FeatureFlag Create(Guid companyId, string key, string name, string? description, int rolloutPercentage = MaxRolloutPercentage)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Feature flag key is required.", nameof(key));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Feature flag name is required.", nameof(name));
        ValidatePercentage(rolloutPercentage);

        var flag = new FeatureFlag
        {
            CompanyId = companyId,
            Key = key.Trim(),
            Name = name.Trim(),
            Description = NormalizeDescription(description),
            IsEnabled = true,
            RolloutPercentage = rolloutPercentage,
        };

        flag.Raise(new FeatureFlagCreated(flag.Id, companyId, flag.Key));
        return flag;
    }

    /// <summary>Updates the mutable display fields. Key and CompanyId are the immutable tenant-scoped business key and stay fixed after creation, same convention as CostCenter.Code.</summary>
    public void UpdateDetails(string name, string? description, int rolloutPercentage)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Feature flag name is required.", nameof(name));
        ValidatePercentage(rolloutPercentage);

        Name = name.Trim();
        Description = NormalizeDescription(description);
        RolloutPercentage = rolloutPercentage;
    }

    /// <summary>Flips IsEnabled — a dedicated fast-path action, separate from UpdateDetails (same split as PluginInstallation's Enable/Disable, or NonConformanceReport's UpdateStatus kept apart from a general edit).</summary>
    public void Toggle() => IsEnabled = !IsEnabled;

    /// <summary>
    /// The runtime evaluation logic behind IsFeatureEnabledQuery. If the flag is disabled,
    /// it is off for everyone regardless of RolloutPercentage. If enabled and no
    /// evaluationId is supplied, the flag is simply on for the whole company (there is
    /// nothing to hash a per-caller bucket from). If enabled and an evaluationId is
    /// supplied (e.g. a UserId), a deterministic hash of Key+evaluationId decides whether
    /// that particular caller falls inside RolloutPercentage — the same evaluationId always
    /// yields the same in/out result for this flag, so one caller never flickers between
    /// "on" and "off" across requests or app restarts. 0% is always off even though
    /// IsEnabled is true; 100% (the default) is always on.
    /// </summary>
    public bool Evaluate(string? evaluationId)
    {
        if (!IsEnabled)
            return false;
        if (string.IsNullOrWhiteSpace(evaluationId))
            return true;
        if (RolloutPercentage <= MinRolloutPercentage)
            return false;
        if (RolloutPercentage >= MaxRolloutPercentage)
            return true;

        return StableHashBucket(Key, evaluationId) < RolloutPercentage;
    }

    private static void ValidatePercentage(int rolloutPercentage)
    {
        if (rolloutPercentage < MinRolloutPercentage || rolloutPercentage > MaxRolloutPercentage)
            throw new ArgumentOutOfRangeException(nameof(rolloutPercentage), "Rollout percentage must be between 0 and 100.");
    }

    private static string? NormalizeDescription(string? description) =>
        string.IsNullOrWhiteSpace(description) ? null : description.Trim();

    /// <summary>
    /// Deterministic hash-mod-100 bucketing. SHA-256 rather than string.GetHashCode() —
    /// .NET randomizes string hash codes per process by default (hash-flood mitigation),
    /// which would make the same caller flip between buckets across app restarts. This is
    /// deliberately just the one calculation — no sticky-assignment table, no real
    /// experimentation platform — per the "keep this simple" scope for this feature.
    /// </summary>
    private static int StableHashBucket(string key, string evaluationId)
    {
        var bytes = Encoding.UTF8.GetBytes($"{key}:{evaluationId}");
        var hash = SHA256.HashData(bytes);
        var bucket = BitConverter.ToUInt32(hash, 0);
        return (int)(bucket % 100);
    }
}
