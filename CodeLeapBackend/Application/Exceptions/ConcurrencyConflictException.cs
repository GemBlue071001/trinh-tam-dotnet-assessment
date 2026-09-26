namespace Application.Exceptions;

public sealed class ConcurrencyConflictException : Exception
{
    public ConcurrencyConflictException() : base("The resource was modified by another request.")
    {
    }
}
