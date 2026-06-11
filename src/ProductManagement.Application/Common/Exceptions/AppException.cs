namespace ProductManagement.Application.Common.Exceptions;

/// <summary>
/// Base type for "smart" application exceptions that carry the HTTP status code
/// and problem-details title they should map to. The global exception handler
/// reads <see cref="StatusCode"/> and <see cref="Title"/> instead of switching
/// on concrete types, so adding a new failure mode is just a new subclass.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    /// <summary>The HTTP status code this exception maps to.</summary>
    public abstract int StatusCode { get; }

    /// <summary>The human-readable problem-details title for this exception.</summary>
    public abstract string Title { get; }
}
