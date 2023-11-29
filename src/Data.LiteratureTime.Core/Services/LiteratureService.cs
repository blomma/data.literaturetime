namespace Data.LiteratureTime.Core.Services;

using System.Security.Cryptography;
using Crypto;
using Interfaces;
using Markdig;
using Models;

public class LiteratureService(ILiteratureProvider literatureProvider) : ILiteratureService
{
    public List<LiteratureTime> GetLiteratureTimes()
    {
        var literatureTimeImports = literatureProvider.ImportLiteratureTimes();

        using var sha256Hash = SHA256.Create();

        List<LiteratureTime> literatureTimes = new(literatureTimeImports.Count());

        foreach (
            var (time, timeQuote, quote, title, author, gutenbergReference) in literatureTimeImports
        )
        {
            var hash = Hashing.GetHash(
                sha256Hash,
                $"{time}{timeQuote}{quote}{title}{author}{gutenbergReference}"
            );

            var qi = quote.IndexOf(timeQuote, StringComparison.InvariantCultureIgnoreCase);
            var quoteFirst = qi > 0 ? quote[..qi] : "";
            var quoteLast = quote[(qi + timeQuote.Length)..];
            var quoteTime = quote[qi..(qi + timeQuote.Length)];

            literatureTimes.Add(
                new LiteratureTime(
                    time,
                    quoteFirst.Replace("\n", " "),
                    quoteTime.Replace("\n", " "),
                    quoteLast.Replace("\n", " "),
                    title,
                    author,
                    gutenbergReference,
                    hash
                )
            );
        }

        return literatureTimes;
    }

    private static string SmartyPants(string input)
    {
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
