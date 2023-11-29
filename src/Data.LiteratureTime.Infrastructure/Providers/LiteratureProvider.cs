namespace Data.LiteratureTime.Infrastructure.Providers;

using System.Text.Encodings.Web;
using System.Text.Json;
using Data.LiteratureTime.Core.Interfaces;
using Data.LiteratureTime.Core.Models;

public class LiteratureProvider : ILiteratureProvider
{
    private static readonly JsonSerializerOptions jsonSerializerOptions =
        new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public IEnumerable<LiteratureTimeImport> ImportLiteratureTimes()
    {
        List<LiteratureTimeImport> literatureTimeImports =  [ ];
        var files = Directory.EnumerateFiles("Data", "*.json", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var content = File.ReadAllText(file);
            var result = JsonSerializer.Deserialize<List<LiteratureTimeImport>>(
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
