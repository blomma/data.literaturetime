namespace Data.LiteratureTime.Core.Interfaces.v2;

public interface ILiteratureProvider
{
    Task<string[]> GetLiteratureTimesAsync();
}
