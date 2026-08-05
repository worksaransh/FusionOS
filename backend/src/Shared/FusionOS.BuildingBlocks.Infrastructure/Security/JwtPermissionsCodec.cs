using System.IO.Compression;
using System.Text;

namespace FusionOS.BuildingBlocks.Infrastructure.Security;

/// <summary>
/// Packs/unpacks a user's full permission-code list into a single gzip+base64url
/// JWT claim value, instead of one JWT claim per permission code. See
/// JwtTokenService's class doc comment for why: a role holding every catalog
/// permission previously produced an Authorization header large enough to trip
/// Node/Vite dev-server's default request-header-size limit (HTTP 431) once the
/// permission catalog passed a few hundred codes, and sat uncomfortably close to
/// common reverse-proxy header-buffer limits in front of a real deployment.
/// Gzip specifically (not just joining into one delimited string) because
/// permission codes are heavily prefix-redundant ("finance.account.create",
/// "finance.account.read", ...) - shared by both JwtTokenService (encode) and
/// HttpCurrentUserContext (decode) so the format only needs to change in one place.
/// </summary>
public static class JwtPermissionsCodec
{
    private const char Delimiter = ',';

    public static string Encode(IReadOnlyCollection<string> permissionCodes)
    {
        if (permissionCodes.Count == 0)
            return string.Empty;

        var joined = string.Join(Delimiter, permissionCodes);
        var rawBytes = Encoding.UTF8.GetBytes(joined);

        using var output = new MemoryStream();
        using (var gzip = new GZipStream(output, CompressionLevel.SmallestSize, leaveOpen: true))
            gzip.Write(rawBytes, 0, rawBytes.Length);

        return Convert.ToBase64String(output.ToArray())
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }

    public static IReadOnlyCollection<string> Decode(string? encoded)
    {
        if (string.IsNullOrEmpty(encoded))
            return Array.Empty<string>();

        var base64 = encoded.Replace('-', '+').Replace('_', '/');
        var padded = base64.Length % 4 == 0 ? base64 : base64 + new string('=', 4 - (base64.Length % 4));
        var compressedBytes = Convert.FromBase64String(padded);

        using var input = new MemoryStream(compressedBytes);
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var reader = new StreamReader(gzip, Encoding.UTF8);
        var joined = reader.ReadToEnd();

        return joined.Length == 0
            ? Array.Empty<string>()
            : joined.Split(Delimiter, StringSplitOptions.RemoveEmptyEntries);
    }
}
