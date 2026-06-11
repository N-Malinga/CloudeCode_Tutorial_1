using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.Common.Exceptions;
using ProductManagement.Domain.Common;
using ValidationException = ProductManagement.Application.Common.Exceptions.ValidationException;

namespace ProductManagement.API.ExceptionHandlers;

/// <summary>
/// Central <see cref="IExceptionHandler"/> that turns exceptions into RFC 7807
/// problem-details responses. In .NET 10 the exception-handler middleware
/// suppresses its own diagnostics once this returns <c>true</c>, so the log
/// written here is the single record of the failure (no duplicate logs).
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // One trace id, shared between the log entry and the response body
        // (CustomizeProblemDetails copies it into the payload for the client).
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var problemDetails = MapToProblemDetails(exception, traceId);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problemDetails,
        });
    }

    private ProblemDetails MapToProblemDetails(Exception exception, string traceId)
    {
        switch (exception)
        {
            // Field-level validation: surface the per-property errors (400).
            case ValidationException validation:
                logger.LogWarning(
                    "Validation failed. TraceId: {TraceId}", traceId);
                return new ValidationProblemDetails(validation.Errors.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
                {
                    Status = validation.StatusCode,
                    Title = validation.Title,
                    Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                };

            // Smart application exceptions carry their own status + title.
            case AppException appException:
                logger.LogWarning(
                    "Handled application exception {ExceptionType}. TraceId: {TraceId}",
                    appException.GetType().Name, traceId);
                return new ProblemDetails
                {
                    Status = appException.StatusCode,
                    Title = appException.Title,
                    Detail = appException.Message,
                };

            // Domain-invariant violation (422). Lives in the Domain layer, so it
            // can't derive from AppException; mapped explicitly here.
            case DomainException domainException:
                logger.LogWarning(
                    "Domain rule violation. TraceId: {TraceId}", traceId);
                return new ProblemDetails
                {
                    Status = StatusCodes.Status422UnprocessableEntity,
                    Title = "Domain rule violation",
                    Detail = domainException.Message,
                };

            // Anything else is unexpected: log the full exception and hide the
            // technical detail from clients outside Development.
            default:
                logger.LogError(
                    exception, "Unhandled exception. TraceId: {TraceId}", traceId);
                return new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = environment.IsDevelopment()
                        ? exception.ToString()
                        : "An unexpected error occurred. Please reference the trace id when contacting support.",
                };
        }
    }
}
