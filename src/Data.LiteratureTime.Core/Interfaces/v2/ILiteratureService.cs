namespace Data.LiteratureTime.Core.Interfaces.v2;

using Data.LiteratureTime.Core.Models;

public interface ILiteratureService
{
    Task<List<LiteratureTime>> GetLiteratureTimesAsync();
}
