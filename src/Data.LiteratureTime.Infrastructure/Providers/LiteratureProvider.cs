namespace Data.LiteratureTime.Infrastructure.Providers;

using Data.LiteratureTime.Core.Interfaces;

public class LiteratureProvider : ILiteratureProvider
{
    public Task<string[]> GetLiteratureTimesAsync()
    {
        var relativePathToFile = GetPathToQuotes();
        return File.ReadAllLinesAsync(relativePathToFile);
    }

    private static string GetPathToQuotes()
    {
        var separator = Path.DirectorySeparatorChar;
        return $"Data{separator}v2{separator}litclock_annotated.csv";
    }
}
