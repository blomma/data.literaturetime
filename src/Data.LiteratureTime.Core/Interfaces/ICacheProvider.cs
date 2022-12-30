namespace Data.LiteratureTime.Core.Interfaces;

public interface ICacheProvider
{
    Task<bool> SetAsync<T>(string key, T data, TimeSpan? expiration = null);
    Task<bool> ExistsAsync(string key);
}
