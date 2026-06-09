namespace TmsApi.Exceptions;

public class TmsDatabaseException : Exception
{
    public TmsDatabaseException(string message)
        : base(message)
    {
    }
}