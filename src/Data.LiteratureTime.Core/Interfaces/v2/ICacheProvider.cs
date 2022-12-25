namespace Data.LiteratureTime.Core.Interfaces.v2;

public interface ICacheProvider
{
    Task<bool> SetAsync<T>(string key, T data, TimeSpan? expiration = null);
    Task<bool> ExistsAsync(string key);
}
