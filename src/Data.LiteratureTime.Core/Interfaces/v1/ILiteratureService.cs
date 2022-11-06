namespace Data.LiteratureTime.Core.Interfaces.v1;

using Data.LiteratureTime.Core.Models;

public interface ILiteratureService
{
    Task<List<LiteratureTime>> GetLiteratureTimesAsync();
}
