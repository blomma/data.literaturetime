namespace Data.LiteratureTime.Core.Interfaces;

public interface IBusProvider
{
    Task PublishAsync(string channel, string message);
}
