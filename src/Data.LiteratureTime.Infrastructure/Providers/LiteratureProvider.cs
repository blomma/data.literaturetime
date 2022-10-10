namespace Data.LiteratureTime.Infrastructure.Providers;

using System.Collections.Generic;
using Data.LiteratureTime.Core.Interfaces;

public class LiteratureProvider : ILiteratureProvider
{
    public List<string> GetLiteratureTimes()
    {
        return File.ReadAllLines("litclock_annotated.csv").ToList();
    }

    public Task<string[]> GetLiteratureTimesAsync()
    {
        return File.ReadAllLinesAsync("litclock_annotated.csv");
    }
}
