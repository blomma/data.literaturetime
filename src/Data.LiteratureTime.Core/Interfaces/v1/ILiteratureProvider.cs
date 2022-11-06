namespace Data.LiteratureTime.Core.Interfaces.v1;

public interface ILiteratureProvider
{
    public List<string> GetLiteratureTimes();
    Task<string[]> GetLiteratureTimesAsync();
}
