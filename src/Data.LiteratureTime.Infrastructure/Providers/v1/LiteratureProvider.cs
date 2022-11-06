namespace Data.LiteratureTime.Infrastructure.Providers.v1;

using System.Collections.Generic;
using Data.LiteratureTime.Core.Interfaces.v1;

public class LiteratureProvider : ILiteratureProvider
{
    public List<string> GetLiteratureTimes()
    {
        return File.ReadAllLines("litclock_annotated.csv").ToList();
    }

    public Task<string[]> GetLiteratureTimesAsync()
    {
        var relativePathToFile = GetPathToQuotes();
        return File.ReadAllLinesAsync(relativePathToFile);
    }

    private static string GetPathToQuotes()
    {
        var separator = Path.DirectorySeparatorChar;
        return $"Data{separator}v1{separator}litclock_annotated.csv";
    }
}
