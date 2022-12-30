namespace Data.LiteratureTime.Core.Interfaces;

public interface ILiteratureProvider
{
    Task<string[]> GetLiteratureTimesAsync();
}
