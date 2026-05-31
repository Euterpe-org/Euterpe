using System.Security.Cryptography;
using System.Text;

namespace Euterpe.Core.Utils;

public sealed record PkcePair(string Verifier, string Challenge)
{
    /// <summary>
    ///     Generate a PKCE verifier/challenge pair (RFC 7636, S256).
    /// </summary>
    public static PkcePair Generate()
    {
        var verifier = RandomNumberGenerator.GetBytes(32).ToBase64Url();
        var challenge = SHA256.HashData(Encoding.ASCII.GetBytes(verifier)).ToBase64Url();
        return new PkcePair(verifier, challenge);
    }
}