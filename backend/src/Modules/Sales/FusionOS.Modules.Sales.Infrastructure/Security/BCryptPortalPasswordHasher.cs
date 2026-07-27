using FusionOS.Modules.Sales.Application.PortalUsers.Contracts;

namespace FusionOS.Modules.Sales.Infrastructure.Security;

/// <summary>
/// Self-contained mirror of Core.Infrastructure.Security.BCryptPasswordHasher
/// (same BCrypt.Net-Next default work factor, same fail-closed behavior on a
/// corrupt hash) - see IPortalPasswordHasher's doc comment for why Sales
/// cannot reference Core's own implementation directly.
/// </summary>
public sealed class BCryptPortalPasswordHasher : IPortalPasswordHasher
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
            return false;
        }
    }
}
