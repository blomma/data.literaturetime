namespace Data.LiteratureTime.Core.Services.v2;

using System.Security.Cryptography;
using System.Text;
using Data.LiteratureTime.Core.Interfaces.v2;
using Data.LiteratureTime.Core.Models;
using Markdig;

public class LiteratureService : ILiteratureService
{
    private readonly ILiteratureProvider _literatureProvider;

    public LiteratureService(ILiteratureProvider literatureProvider)
    {
        _literatureProvider = literatureProvider;
    }

    public async Task<List<LiteratureTime>> GetLiteratureTimesAsync()
    {
        var rows = await _literatureProvider.GetLiteratureTimesAsync();
        using SHA256 sha256Hash = SHA256.Create();

        List<LiteratureTime> literatureTimes = new();
        int exceptionCount = 0;
        foreach (var row in rows)
        {
            try
            {
                var (time, literatureTime, quote, title, author) = ParseRow(row);
                var hash = GetHash(sha256Hash, $"{time}{literatureTime}{quote}{title}{author}");

                var qi = quote.ToLowerInvariant().IndexOf(literatureTime.ToLowerInvariant());
                var quoteFirst = qi > 0 ? quote[..qi] : "";
                var quoteLast = quote[(qi + literatureTime.Length)..];

                literatureTimes.Add(
                    new LiteratureTime(
                        time,
                        quoteFirst,
                        literatureTime,
                        quoteLast,
                        title,
                        author,
                        hash
                    )
                );
            }
            catch
            {
                exceptionCount += 1;
            }
        }

        return literatureTimes;
    }

    private static (
        string time,
        string literatureTime,
        string quote,
        string title,
        string author
    ) ParseRow(string row)
    {
        var entries = row.Split("|");

        var time = entries[0].Trim();

        var literatureTime = entries[1].Trim();
        literatureTime = SmartyPants(literatureTime);

        var quote = entries[2].Trim();
        quote = SmartyPants(quote);

        var title = entries[3].Trim();

        var author = entries[4].Trim();

        return (time, literatureTime, quote, title, author);
    }

    private static string SmartyPants(string input)
    {
        input = input.Replace("<br/>", "\n");

        var pipeline = new MarkdownPipelineBuilder().UseSmartyPants().Build();
        var result = Markdown.ToPlainText(input, pipeline);

        result = result.Replace("&lsquo;", "‘");
        result = result.Replace("&rsquo;", "’");
        result = result.Replace("&ldquo;", "“");
        result = result.Replace("&rdquo;", "”");
        result = result.Replace("&laquo;", "«");
        result = result.Replace("&raquo;", "»");
        result = result.Replace("&hellip;", "…");
        result = result.Replace("&ndash;", "–");
        result = result.Replace("&mdash;", "—");

        result = result.TrimEnd();

        return result;
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
