namespace ProductManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request is well-formed but cannot be fulfilled for a reason
/// that isn't a field-level validation failure (use <see cref="ValidationException"/>
/// for those) or a domain-invariant violation.
/// </summary>
public sealed class BadRequestException : AppException
{
    public BadRequestException(string message)
        : base(message)
    {
    }

    public override int StatusCode => 400;
    public override string Title => "Bad request";
}
