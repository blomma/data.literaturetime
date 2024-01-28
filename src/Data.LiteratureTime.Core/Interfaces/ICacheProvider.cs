namespace Data.LiteratureTime.Core.Interfaces;

public interface ICacheProvider
{
    Task SetAsync<T>(string key, T data, TimeSpan? expiration = null);
    Task SetAsync<T>(string key, List<T> data, TimeSpan? expiration = null);
    Task<bool> ExistsAsync(string key);
}
