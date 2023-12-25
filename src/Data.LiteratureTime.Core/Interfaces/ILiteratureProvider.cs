namespace Data.LiteratureTime.Core.Interfaces;

public interface ILiteratureProvider
{
    IEnumerable<Models.LiteratureTime> ImportLiteratureTimes();
}
