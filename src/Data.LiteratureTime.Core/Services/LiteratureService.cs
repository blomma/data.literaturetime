namespace Data.LiteratureTime.Core.Services;

using System.Security.Cryptography;
using System.Text;
using Data.LiteratureTime.Core.Interfaces;
using Data.LiteratureTime.Core.Models;

public class LiteratureService : ILiteratureService
{
    private readonly ILiteratureProvider _literatureProvider;

    public LiteratureService(ILiteratureProvider literatureProvider)
    {
        _literatureProvider = literatureProvider;
    }

    public async Task<List<LiteratureTime>> GetLiteratureTimesAsync()
    {
        var result = await _literatureProvider.GetLiteratureTimesAsync();
        using SHA256 sha256Hash = SHA256.Create();

        return result
            .Select(r =>
            {
                var a = r.Split("|");
                var time = a[0].Trim();
                var literatureTime = a[1].Trim();
                var quote = a[2].Trim();
                var title = a[3].Trim();
                var author = a[4].Trim();

                var hash = GetHash(sha256Hash, $"{time}{literatureTime}{quote}{title}{author}");

                var qi = quote.ToLowerInvariant().IndexOf(literatureTime.ToLowerInvariant());
                var quoteFirst = qi > 0 ? quote[..qi] : "";
                var quoteTime = quote[qi..(qi + literatureTime.Length)];
                var quoteLast = quote[(qi + literatureTime.Length)..];

                return new LiteratureTime(
                    time,
                    quoteFirst,
                    quoteTime,
                    quoteLast,
                    title,
                    author,
                    hash
                );
            })
            .ToList();
    }

    private static string GetHash(HashAlgorithm hashAlgorithm, string input)
    {
        byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

        var stringBuilder = new StringBuilder();

        for (int i = 0; i < data.Length; i++)
        {
            stringBuilder.Append(data[i].ToString("x2"));
        }

        return stringBuilder.ToString();
    }
}
