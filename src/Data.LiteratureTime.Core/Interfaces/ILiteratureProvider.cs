namespace Data.LiteratureTime.Core.Interfaces;

public interface ILiteratureProvider
{
    public List<string> GetLiteratureTimes();
    Task<string[]> GetLiteratureTimesAsync();
}
