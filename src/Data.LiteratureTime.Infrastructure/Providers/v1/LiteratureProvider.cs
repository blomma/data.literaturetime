namespace Data.LiteratureTime.Infrastructure.Providers.v1;

using Data.LiteratureTime.Core.Interfaces.v1;

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
        return $"Data{separator}v1{separator}litclock_annotated.csv";
    }
}
