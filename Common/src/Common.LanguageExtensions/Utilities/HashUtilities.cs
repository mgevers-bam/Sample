using System.Security.Cryptography;
using System.Text;

namespace Common.LanguageExtensions.Utilities;

public static class HashUtilities
{
    public static string ComputeSHA256Hash(string value)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexStringLower(hash);
    }
}
