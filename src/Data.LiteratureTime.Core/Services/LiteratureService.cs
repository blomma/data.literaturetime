using Data.LiteratureTime.Core.Interfaces;

namespace Data.LiteratureTime.Core.Services;

public class LiteratureService(ILiteratureProvider literatureProvider) : ILiteratureService
{
    public List<Models.LiteratureTime> GetLiteratureTimes()
    {
        return literatureProvider.ImportLiteratureTimes();
    }
}
