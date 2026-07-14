using FusionOS.SharedKernel;

namespace FusionOS.Modules.Core.Domain.Identity;

/// <summary>
/// A User is a global identity (per 07_SECURITY.md) that may hold a role in
/// multiple companies via <see cref="UserCompanyRole"/> — it is not itself
/// tenant-scoped to a single CompanyId.
/// </summary>
public sealed class User : AuditableEntity
{
    public string Email { get; private set; } = default!;
    public string FullName { get; private set; } = default!;
    public string PasswordHash { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;
    public bool TwoFactorEnabled { get; private set; }

    private User() { }

    public static User Register(string email, string fullName, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("A valid email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            FullName = fullName.Trim(),
            PasswordHash = passwordHash,
        };
    }

    public void EnableTwoFactor() => TwoFactorEnabled = true;
    public void Deactivate() => IsActive = false;
}
