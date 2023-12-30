using System.Text.Encodings.Web;
using System.Text.Json;
using Data.LiteratureTime.Core.Interfaces;

namespace Data.LiteratureTime.Infrastructure.Providers;

public class LiteratureProvider : ILiteratureProvider
{
    private static readonly JsonSerializerOptions jsonSerializerOptions =
        new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            PropertyNameCaseInsensitive = true
        };

    public IEnumerable<Core.Models.LiteratureTime> ImportLiteratureTimes()
    {
        List<Core.Models.LiteratureTime> literatureTimeImports = [];
        var files = Directory.EnumerateFiles("Quotes", "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var result = JsonSerializer.Deserialize<List<Core.Models.LiteratureTime>>(
                content,
                jsonSerializerOptions
            );

            if (result != null)
            {
                literatureTimeImports.AddRange(result);
            }
        }

        return literatureTimeImports;
    }
}
