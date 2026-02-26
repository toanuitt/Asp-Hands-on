using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;

public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
            return false;

        _logger.LogWarning("Validation error occurred");

        var errors = validationException.Errors
            .Select(e => new
            {
                field = e.PropertyName,
                message = e.ErrorMessage
            });

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(new
        {
            message = "Validation failed",
            errors
        }, cancellationToken);

        return true; 
    }
}