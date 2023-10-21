namespace Data.LiteratureTime.Core.Crypto;

using System.Security.Cryptography;
using System.Text;

public static class Hashing
{
    public static string GetHash(HashAlgorithm hashAlgorithm, string input)
    {
        var data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        var stringBuilder = new StringBuilder();
        foreach (var t in data)
        {
            stringBuilder.Append(t.ToString("x2"));
        }

        return stringBuilder.ToString();
    }
}
