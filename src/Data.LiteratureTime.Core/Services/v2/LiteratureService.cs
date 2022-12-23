namespace Data.LiteratureTime.Core.Services.v2;

using System.Security.Cryptography;
using Data.LiteratureTime.Core.Crypto;
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

        List<LiteratureTime> literatureTimes = new(rows.Length);
        foreach (var row in rows)
        {
            var (time, literatureTime, quote, title, author) = ParseRow(row);
            var hash = Hashing.GetHash(sha256Hash, $"{time}{literatureTime}{quote}{title}{author}");

            var qi = quote.IndexOf(literatureTime, StringComparison.InvariantCultureIgnoreCase);
            var quoteFirst = qi > 0 ? quote[..qi] : "";
            var quoteLast = quote[(qi + literatureTime.Length)..];

            literatureTimes.Add(
                new LiteratureTime(time, quoteFirst, literatureTime, quoteLast, title, author, hash)
            );
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
        input = input.Replace("<br/>", "\n", StringComparison.InvariantCultureIgnoreCase);

        var pipeline = new MarkdownPipelineBuilder().UseSmartyPants().Build();
        var result = Markdown.ToPlainText(input, pipeline);

        result = result.Replace("&lsquo;", "‘", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&rsquo;", "’", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&ldquo;", "“", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&rdquo;", "”", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&laquo;", "«", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&raquo;", "»", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&hellip;", "…", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&ndash;", "–", StringComparison.InvariantCultureIgnoreCase);
        result = result.Replace("&mdash;", "—", StringComparison.InvariantCultureIgnoreCase);

        result = result.TrimEnd();

        return result;
    }
}
