using FusionOS.Modules.Core.Application.Auth.Contracts;

namespace FusionOS.Modules.Core.Infrastructure.Security;

/// <summary>BCrypt with the library's default work factor (currently 11) - 07_SECURITY.md.</summary>
public sealed class BCryptPasswordHasher : IPasswordHasher
{
    public string Hash(string plainTextPassword) => BCrypt.Net.BCrypt.HashPassword(plainTextPassword);

    public bool Verify(string plainTextPassword, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(plainTextPassword, hash);
        }
        catch (BCrypt.Net.SaltParseException)
        {
            // A corrupt/unrecognized hash must fail closed, never throw past the auth boundary.
            return false;
        }
    }
}
