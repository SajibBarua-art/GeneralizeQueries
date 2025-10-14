namespace GeneralizeQueries.Core.Exceptions;

public class DuplicateUniqNameException : Exception
{
    public DuplicateUniqNameException(string message) : base(message)
    {
    }

    public DuplicateUniqNameException(
        string message,
        Exception innerException) : base(message, innerException)
    {
    }
}