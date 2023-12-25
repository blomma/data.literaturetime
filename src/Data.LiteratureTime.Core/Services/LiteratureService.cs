using Data.LiteratureTime.Core.Interfaces;

namespace Data.LiteratureTime.Core.Services;

public class LiteratureService(ILiteratureProvider literatureProvider) : ILiteratureService
{
    public IEnumerable<Models.LiteratureTime> GetLiteratureTimes()
    {
        return literatureProvider.ImportLiteratureTimes();
    }
}
