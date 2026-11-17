using System;
using System.Security.Cryptography;
using System.Text;

namespace Vsky.Api.Authorization
{
    /// <summary>
    /// Ties a token to the user's security stamp, which ASP.NET Identity changes when a password
    /// changes, so old tokens stop working. The token holds a hash, as tokens are readable.
    /// Tokens issued before this existed have no such claim and stay valid until they expire.
    /// </summary>
    public static class PublicEndpointLimit
    {
        public const string PolicyName = "public-endpoint";
        public const int RequestsPerMinute = 60;
    }

    public static class SessionStamp
    {
        public const string ClaimType = "session_stamp";

        public static string For(string securityStamp)
        {
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(securityStamp ?? string.Empty)));
        }
    }
}
