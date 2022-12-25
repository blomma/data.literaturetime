namespace Data.LiteratureTime.Core.Interfaces.v2;

public interface ICacheProvider
{
    Task Set<T>(string key, T data, TimeSpan? expiration = null);
    Task<bool> Exists(string key);
}
