namespace SmartLightSystem.Exceptions;

public sealed class TimeProviderException : Exception
{
    public TimeProviderException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
