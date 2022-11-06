namespace Data.LiteratureTime.Core.Interfaces.v1;

public interface ILiteratureProvider
{
    Task<string[]> GetLiteratureTimesAsync();
}
