using Data.LiteratureTime.Core.Models;

namespace Data.LiteratureTime.Core.Interfaces;

public interface ILiteratureProvider
{
    IEnumerable<LiteratureTimeImport> ImportLiteratureTimes();
}
