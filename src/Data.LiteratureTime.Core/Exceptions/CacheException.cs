namespace Data.LiteratureTime.Core.Exceptions;

public class CacheException : Exception
{
    public CacheException() { }

    public CacheException(string message)
        : base(message) { }

    public CacheException(string message, Exception innerException)
        : base(message, innerException) { }
}
