namespace Data.LiteratureTime.Core.Interfaces;

using Data.LiteratureTime.Core.Models;

public interface ILiteratureService
{
    Task<List<LiteratureTime>> GetLiteratureTimesAsync();
}
